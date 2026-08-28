using MesaPartesDigital.Models;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MesaPartesDigital.Services
{
    public class DocumentoAdjuntoServices
    {
        private readonly IConfiguration _configuration;
        private readonly IJSRuntime _js;
        public DocumentoAdjuntoServices(IConfiguration configuration, IJSRuntime js)
        {
            _configuration = configuration;
            _js = js;
        }

        //Documento Principal
        public async Task<ArchivoAdjunto?> ProcesarArchivoSeleccionadoAsync(InputFileChangeEventArgs e, string nombrePersonalizado = "DOCUMENTO PRINCIPAL.pdf")
        {
            var archivoSeleccionado = e.File;
            if (archivoSeleccionado == null) return null;

            // 1. Validar extensión PDF
            if (!archivoSeleccionado.Name.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                await _js.InvokeVoidAsync("Swal.fire", "Formato no permitido", "Solo se permiten documentos en formato PDF.", "warning");
                return null;
            }

            // 2. Obtener límite desde la configuración
            int maxMb = _configuration.GetValue<int>("ConfiguracionAdjuntos:PesoMaximoMB");
            if (maxMb <= 0) maxMb = 50; // Seguridad por si no está configurado

            long maxFileSize = maxMb * 1024 * 1024L;

            // 3. Validar archivo vacío
            if (archivoSeleccionado.Size <= 0)
            {
                await _js.InvokeVoidAsync("Swal.fire", "Archivo vacío", "El archivo seleccionado no contiene información.", "warning");
                return null;
            }

            // 4. Validación estricta de tamaño
            if (archivoSeleccionado.Size > maxFileSize)
            {
                double tamanoActualMb = Math.Round((double)archivoSeleccionado.Size / (1024 * 1024), 2);

                await _js.InvokeVoidAsync("Swal.fire", new
                {
                    html = $@"
                            <div style='text-align: left; padding: 10px 5px; font-family: system-ui, -apple-system, sans-serif;'>
                                <div style='display: flex; align-items: center; gap: 14px; margin-bottom: 20px;'>
                                    <div style='background-color: #eff6ff; color: #2563eb; width: 48px; height: 48px; border-radius: 12px; display: flex; align-items: center; justify-content: center; font-size: 24px; flex-shrink: 0; box-shadow: 0 2px 4px rgba(37, 99, 235, 0.1);'>📁</div>
                                    <div>
                                        <h3 style='margin: 0; font-size: 18px; font-weight: 700; color: #1e293b;'>Capacidad Excedida</h3>
                                        <p style='margin: 2px 0 0 0; font-size: 13px; color: #64748b;'>Mesa de Partes Virtual</p>
                                    </div>
                                </div>
                                <p style='font-size: 14px; color: #475569; line-height: 1.5; margin-bottom: 20px;'>El documento que intenta adjuntar supera el tamaño máximo permitido para su correcta recepción en el sistema.</p>
                                <div style='background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 12px; padding: 16px; margin-bottom: 20px;'>
                                    <div style='display: flex; justify-content: space-between; align-items: center; margin-bottom: 10px; font-size: 14px;'>
                                        <span style='color: #64748b;'>Peso de su archivo:</span>
                                        <span style='font-weight: 600; color: #ef4444; background: #fee2e2; padding: 2px 8px; border-radius: 6px;'>{tamanoActualMb} MB</span>
                                    </div>
                                    <div style='display: flex; justify-content: space-between; align-items: center; font-size: 14px; border-top: 1px dashed #cbd5e1; padding-top: 10px;'>
                                        <span style='color: #64748b;'>Límite máximo permitido:</span>
                                        <span style='font-weight: 600; color: #0f172a;'>{maxMb} MB</span>
                                    </div>
                                </div>
                                <div style='display: flex; align-items: center; gap: 8px; font-size: 12px; color: #0284c7; background: #e0f2fe; padding: 10px 12px; border-radius: 8px;'>
                                    <span>💡</span>
                                    <span>Le sugerimos optimizar o fraccionar su documento antes de reintentarlo.</span>
                                </div>
                            </div>",
                    showConfirmButton = true,
                    confirmButtonText = "Entendido, volver a intentar",
                    confirmButtonColor = "#1e293b",
                    background = "#ffffff",
                    backdrop = "rgba(15, 23, 42, 0.5)",
                    customClass = new { popup = "rounded-2xl shadow-xl border border-slate-100" }
                });

                return null;
            }

            try
            {
                await using var stream = archivoSeleccionado.OpenReadStream(maxFileSize);
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);

                if (ms.Length <= 0)
                {
                    await _js.InvokeVoidAsync("Swal.fire", "Error", "El archivo no pudo ser leído correctamente.", "error");
                    return null;
                }

                if (ms.Length != archivoSeleccionado.Size)
                {
                    Console.WriteLine($"[Carga incompleta principal] Archivo: {archivoSeleccionado.Name} | Esperado: {archivoSeleccionado.Size} bytes | Recibido: {ms.Length} bytes");
                    await _js.InvokeVoidAsync(
                        "Swal.fire",
                        "Carga incompleta",
                        $"El archivo no se cargó completamente. Tamaño esperado: {archivoSeleccionado.Size} bytes. Tamaño recibido: {ms.Length} bytes.",
                        "error");
                    return null;
                }

                // Se asigna el nombre personalizado (o se mantiene el original si se pasa un string vacío o nulo)
                string nombreFinal = !string.IsNullOrEmpty(nombrePersonalizado) ? nombrePersonalizado : archivoSeleccionado.Name;

                return new ArchivoAdjunto
                {
                    Nombre = nombreFinal,
                    Tipo = archivoSeleccionado.ContentType,
                    Tamano = $"{Math.Round((double)archivoSeleccionado.Size / 1024, 2)} KB",
                    Contenido = ms.ToArray()
                };
            }
            catch (IOException ex)
            {
                Console.WriteLine($"[IOException ProcesarArchivoSeleccionadoAsync] {ex}");
                await _js.InvokeVoidAsync("Swal.fire", "Error de lectura", $"No se pudo leer completamente el archivo. {ex.Message}", "error");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error ProcesarArchivoSeleccionadoAsync] {ex}");
                await _js.InvokeVoidAsync("Swal.fire", "Error", $"No se pudo cargar el archivo: {ex.Message}", "error");
                return null;
            }
        }

        //Anexos
        //public async Task<List<ArchivoAdjunto>> ProcesarAnexosSeleccionadosAsync(InputFileChangeEventArgs e, List<ArchivoAdjunto> archivosActuales, int maxAnexosPermitidos = 10)
        //{
        //    // 1. Validar si al agregar los nuevos archivos se supera el límite máximo
        //    var archivosSeleccionados = e.GetMultipleFiles(maxAnexosPermitidos).ToList();

        //    if (archivosActuales.Count + archivosSeleccionados.Count > maxAnexosPermitidos)
        //    {
        //        await _js.InvokeVoidAsync("Swal.fire", new
        //        {
        //            html = $@"
        //                    <div style='text-align: left; padding: 10px 5px; font-family: system-ui, -apple-system, sans-serif;'>
        //                        <!-- Cabecera con Icono -->
        //                        <div style='display: flex; align-items: center; gap: 14px; margin-bottom: 20px;'>
        //                            <div style='background-color: #fff1f2; color: #e11d48; width: 48px; height: 48px; border-radius: 12px; display: flex; align-items: center; justify-content: center; font-size: 24px; flex-shrink: 0; box-shadow: 0 2px 4px rgba(225, 29, 72, 0.1);'>
        //                                📑
        //                            </div>
        //                            <div>
        //                                <h3 style='margin: 0; font-size: 18px; font-weight: 700; color: #1e293b;'>Límite de Anexos Alcanzado</h3>
        //                                <p style='margin: 2px 0 0 0; font-size: 13px; color: #64748b;'>Gestión de Archivos Adjuntos</p>
        //                            </div>
        //                        </div>

        //                        <!-- Descripción -->
        //                        <p style='font-size: 14px; color: #475569; line-height: 1.5; margin-bottom: 20px;'>
        //                            Ha excedido la cantidad máxima de archivos adjuntos permitidos para este trámite.
        //                        </p>

        //                        <!-- Caja de Resumen -->
        //                        <div style='background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 12px; padding: 16px; margin-bottom: 20px;'>
        //                            <div style='display: flex; justify-content: space-between; align-items: center; font-size: 14px;'>
        //                                <span style='color: #64748b;'>Máximo permitido:</span>
        //                                <span style='font-weight: 700; color: #1e293b; font-size: 16px;'>{maxAnexosPermitidos} archivos</span>
        //                            </div>
        //                        </div>

        //                        <!-- Nota de ayuda -->
        //                        <div style='display: flex; align-items: center; gap: 8px; font-size: 12px; color: #be123c; background: #fff1f2; padding: 10px 12px; border-radius: 8px;'>
        //                            <span>⚠️</span>
        //                            <span>Por favor, verifique su selección antes de continuar.</span>
        //                        </div>
        //                    </div>
        //                ",
        //            showConfirmButton = true,
        //            confirmButtonText = "Entendido",
        //            confirmButtonColor = "#1e293b",
        //            backdrop = "rgba(15, 23, 42, 0.5)",
        //            customClass = new
        //            {
        //                popup = "rounded-2xl shadow-xl border border-slate-100"
        //            }
        //        });
        //        return archivosActuales;
        //    }

        //    // 2. Obtener peso máximo para anexos (por defecto 20 MB o desde configuración)
        //    int maxMbAnexo = _configuration.GetValue<int>("ConfiguracionAdjuntos:PesoMaximoAnexoMB");
        //    if (maxMbAnexo <= 0) maxMbAnexo = 20;

        //    long maxFileSize = maxMbAnexo * 1024 * 1024L;
        //    var listaProcesada = new List<ArchivoAdjunto>(archivosActuales);

        //    foreach (var archivo in archivosSeleccionados)
        //    {
        //        // 2. VALIDACIÓN DE DUPLICADOS (Por Nombre y Tamaño exacto)
        //        bool yaExiste = listaProcesada.Any(a => a.Nombre.Equals(archivo.Name, StringComparison.OrdinalIgnoreCase) &&
        //                                              a.Tamano == $"{Math.Round((double)archivo.Size / 1024, 2)} KB");
        //        if (yaExiste)
        //        {
        //            await _js.InvokeVoidAsync("Swal.fire", new
        //            {
        //                html = $@"
        //                        <div style='text-align: left; padding: 10px 5px; font-family: system-ui, -apple-system, sans-serif;'>
        //                            <!-- Cabecera con Icono -->
        //                            <div style='display: flex; align-items: center; gap: 14px; margin-bottom: 20px;'>
        //                                <div style='background-color: #fefce8; color: #ca8a04; width: 48px; height: 48px; border-radius: 12px; display: flex; align-items: center; justify-content: center; font-size: 24px; flex-shrink: 0; box-shadow: 0 2px 4px rgba(202, 138, 4, 0.1);'>
        //                                    ⚠️
        //                                </div>
        //                                <div>
        //                                    <h3 style='margin: 0; font-size: 18px; font-weight: 700; color: #1e293b;'>Archivo Duplicado</h3>
        //                                    <p style='margin: 2px 0 0 0; font-size: 13px; color: #64748b;'>Control de Anexos</p>
        //                                </div>
        //                            </div>

        //                            <!-- Descripción -->
        //                            <p style='font-size: 14px; color: #475569; line-height: 1.5; margin-bottom: 20px;'>
        //                                El documento que intenta adjuntar ya fue registrado previamente en la lista de anexos de este trámite.
        //                            </p>

        //                            <!-- Tarjeta de Archivo Duplicado -->
        //                            <div style='background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 12px; padding: 16px; margin-bottom: 20px;'>
        //                                <div style='display: flex; justify-content: space-between; align-items: center; font-size: 14px;'>
        //                                    <span style='color: #64748b;'>Archivo existente:</span>
        //                                    <span style='font-weight: 600; color: #0f172a; max-width: 200px; white-space: nowrap; overflow: hidden; text-overflow: ellipsis;' title='{archivo.Name}'>{archivo.Name}</span>
        //                                </div>
        //                            </div>

        //                            <!-- Nota de ayuda -->
        //                            <div style='display: flex; align-items: center; gap: 8px; font-size: 12px; color: #a16207; background: #fefce8; padding: 10px 12px; border-radius: 8px;'>
        //                                <span>💡</span>
        //                                <span>No es necesario adjuntar el mismo archivo dos veces. Verifique su lista.</span>
        //                            </div>
        //                        </div>
        //                    ",
        //                showConfirmButton = true,
        //                confirmButtonText = "Entendido",
        //                confirmButtonColor = "#1e293b",
        //                backdrop = "rgba(15, 23, 42, 0.5)",
        //                customClass = new
        //                {
        //                    popup = "rounded-2xl shadow-xl border border-slate-100"
        //                }
        //            });
        //            continue;
        //        }

        //        // 3. Validar extensión PDF
        //        if (!archivo.Name.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        //        {
        //            await _js.InvokeVoidAsync("Swal.fire", new
        //            {
        //                html = $@"
        //                        <div style='text-align: left; padding: 10px 5px; font-family: system-ui, -apple-system, sans-serif;'>
        //                            <!-- Cabecera con Icono -->
        //                            <div style='display: flex; align-items: center; gap: 14px; margin-bottom: 20px;'>
        //                                <div style='background-color: #fef2f2; color: #dc2626; width: 48px; height: 48px; border-radius: 12px; display: flex; align-items: center; justify-content: center; font-size: 24px; flex-shrink: 0; box-shadow: 0 2px 4px rgba(220, 38, 38, 0.1);'>
        //                                    📄
        //                                </div>
        //                                <div>
        //                                    <h3 style='margin: 0; font-size: 18px; font-weight: 700; color: #1e293b;'>Formato No Permitido</h3>
        //                                    <p style='margin: 2px 0 0 0; font-size: 13px; color: #64748b;'>Validación de Documento</p>
        //                                </div>
        //                            </div>

        //                            <!-- Descripción -->
        //                            <p style='font-size: 14px; color: #475569; line-height: 1.5; margin-bottom: 20px;'>
        //                                El archivo que intenta adjuntar no corresponde a un formato válido para el trámite.
        //                            </p>

        //                            <!-- Tarjeta de Archivo Rechazado -->
        //                            <div style='background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 12px; padding: 16px; margin-bottom: 20px;'>
        //                                <div style='display: flex; align-items: center; justify-content: space-between; font-size: 14px;'>
        //                                    <span style='color: #64748b;'>Archivo seleccionado:</span>
        //                                    <span style='font-weight: 600; color: #0f172a; max-width: 200px; white-space: nowrap; overflow: hidden; text-overflow: ellipsis;' title='{archivo.Name}'>{archivo.Name}</span>
        //                                </div>
        //                                <div style='display: flex; justify-content: space-between; align-items: center; font-size: 14px; border-top: 1px dashed #cbd5e1; margin-top: 10px; padding-top: 10px;'>
        //                                    <span style='color: #64748b;'>Formato requerido:</span>
        //                                    <span style='font-weight: 600; color: #dc2626; background: #fee2e2; padding: 2px 8px; border-radius: 6px;'>Solo PDF (.pdf)</span>
        //                                </div>
        //                            </div>

        //                            <!-- Nota de ayuda -->
        //                            <div style='display: flex; align-items: center; gap: 8px; font-size: 12px; color: #b91c1c; background: #fef2f2; padding: 10px 12px; border-radius: 8px;'>
        //                                <span>⚠️</span>
        //                                <span>Convierta su documento a PDF antes de volver a adjuntarlo.</span>
        //                            </div>
        //                        </div>
        //                    ",
        //                showConfirmButton = true,
        //                confirmButtonText = "Entendido",
        //                confirmButtonColor = "#1e293b",
        //                backdrop = "rgba(15, 23, 42, 0.5)",
        //                customClass = new
        //                {
        //                    popup = "rounded-2xl shadow-xl border border-slate-100"
        //                }
        //            });
        //            continue;
        //        }

        //        // 4. Validar archivo vacío
        //        if (archivo.Size <= 0)
        //        {
        //            await _js.InvokeVoidAsync("Swal.fire", "Archivo vacío", $"El archivo '{archivo.Name}' no contiene información.", "warning");
        //            continue;
        //        }

        //        // 5. Validar peso de cada archivo
        //        if (archivo.Size > maxFileSize)
        //        {
        //            double tamanoActualMb = Math.Round((double)archivo.Size / (1024 * 1024), 2);
        //            await _js.InvokeVoidAsync("Swal.fire", new
        //            {
        //                html = $@"
        //                        <div style='text-align: left; padding: 10px 5px; font-family: system-ui, -apple-system, sans-serif;'>
        //                            <!-- Cabecera con Icono -->
        //                            <div style='display: flex; align-items: center; gap: 14px; margin-bottom: 20px;'>
        //                                <div style='background-color: #fef3c7; color: #d97706; width: 48px; height: 48px; border-radius: 12px; display: flex; align-items: center; justify-content: center; font-size: 24px; flex-shrink: 0; box-shadow: 0 2px 4px rgba(217, 119, 6, 0.1);'>
        //                                    ⚠️
        //                                </div>
        //                                <div>
        //                                    <h3 style='margin: 0; font-size: 18px; font-weight: 700; color: #1e293b;'>Archivo Demasiado Pesado</h3>
        //                                    <p style='margin: 2px 0 0 0; font-size: 13px; color: #64748b;'>Control de Capacidad de Anexos</p>
        //                                </div>
        //                            </div>

        //                            <!-- Descripción -->
        //                            <p style='font-size: 14px; color: #475569; line-height: 1.5; margin-bottom: 20px;'>
        //                                El archivo que intenta adjuntar excede el peso límite establecido para los anexos de este trámite.
        //                            </p>

        //                            <!-- Tarjeta de Detalles del Archivo -->
        //                            <div style='background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 12px; padding: 16px; margin-bottom: 20px;'>
        //                                <div style='display: flex; justify-content: space-between; align-items: center; font-size: 14px; margin-bottom: 10px;'>
        //                                    <span style='color: #64748b;'>Archivo:</span>
        //                                    <span style='font-weight: 600; color: #0f172a; max-width: 200px; white-space: nowrap; overflow: hidden; text-overflow: ellipsis;' title='{archivo.Name}'>{archivo.Name}</span>
        //                                </div>
        //                                <div style='display: flex; justify-content: space-between; align-items: center; font-size: 14px; margin-bottom: 10px;'>
        //                                    <span style='color: #64748b;'>Peso actual:</span>
        //                                    <span style='font-weight: 600; color: #ef4444; background: #fee2e2; padding: 2px 8px; border-radius: 6px;'>{tamanoActualMb} MB</span>
        //                                </div>
        //                                <div style='display: flex; justify-content: space-between; align-items: center; font-size: 14px; border-top: 1px dashed #cbd5e1; padding-top: 10px;'>
        //                                    <span style='color: #64748b;'>Límite máximo permitido:</span>
        //                                    <span style='font-weight: 600; color: #0f172a;'>{maxMbAnexo} MB</span>
        //                                </div>
        //                            </div>

        //                            <!-- Nota de ayuda -->
        //                            <div style='display: flex; align-items: center; gap: 8px; font-size: 12px; color: #b45309; background: #fef3c7; padding: 10px 12px; border-radius: 8px;'>
        //                                <span>💡</span>
        //                                <span>Optimice o comprima el documento antes de volver a adjuntarlo.</span>
        //                            </div>
        //                        </div>
        //                    ",
        //                showConfirmButton = true,
        //                confirmButtonText = "Entendido",
        //                confirmButtonColor = "#1e293b",
        //                backdrop = "rgba(15, 23, 42, 0.5)",
        //                customClass = new
        //                {
        //                    popup = "rounded-2xl shadow-xl border border-slate-100"
        //                }
        //            });
        //            continue;
        //        }

        //        try
        //        {
        //            await using var stream = archivo.OpenReadStream(maxFileSize);
        //            using var ms = new MemoryStream();
        //            await stream.CopyToAsync(ms);

        //            if (ms.Length <= 0)
        //            {
        //                Console.WriteLine($"[Anexo vacío después de lectura] Archivo: {archivo.Name}");
        //                await _js.InvokeVoidAsync("Swal.fire", "Error de lectura", $"El archivo '{archivo.Name}' no pudo ser leído correctamente.", "error");
        //                continue;
        //            }

        //            if (ms.Length != archivo.Size)
        //            {
        //                Console.WriteLine($"[Carga incompleta de anexo] Archivo: {archivo.Name} | Esperado: {archivo.Size} bytes | Recibido: {ms.Length} bytes");
        //                await _js.InvokeVoidAsync(
        //                    "Swal.fire",
        //                    "Carga incompleta",
        //                    $"El archivo '{archivo.Name}' no se cargó completamente. Tamaño esperado: {archivo.Size} bytes. Tamaño recibido: {ms.Length} bytes.",
        //                    "error");
        //                continue;
        //            }

        //            listaProcesada.Add(new ArchivoAdjunto
        //            {
        //                Nombre = archivo.Name,
        //                Tipo = archivo.ContentType,
        //                Tamano = $"{Math.Round((double)archivo.Size / 1024, 2)} KB",
        //                Contenido = ms.ToArray()
        //            });

        //            Console.WriteLine($"[Anexo cargado correctamente] Archivo: {archivo.Name} | Tamaño: {archivo.Size} bytes");
        //        }
        //        catch (IOException ex)
        //        {
        //            Console.WriteLine($"[IOException ProcesarAnexo] Archivo: {archivo.Name} | {ex}");
        //            await _js.InvokeVoidAsync("Swal.fire", "Error de lectura", $"No se pudo leer completamente el archivo '{archivo.Name}'. {ex.Message}", "error");
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine($"[Error ProcesarAnexo] Archivo: {archivo.Name} | {ex}");
        //            await _js.InvokeVoidAsync("Swal.fire", "Error", $"No se pudo cargar el archivo '{archivo.Name}': {ex.Message}", "error");
        //        }
        //    }

        //    return listaProcesada;
        //}

        public async Task<List<ArchivoAdjunto>> ProcesarAnexosSeleccionadosAsync(InputFileChangeEventArgs e, List<ArchivoAdjunto> archivosActuales, int maxAnexosPermitidos = 10)
        {
            // 1. Validar si al agregar los nuevos archivos se supera el límite máximo
            var archivosSeleccionados = e.GetMultipleFiles(maxAnexosPermitidos).ToList();

            if (archivosActuales.Count + archivosSeleccionados.Count > maxAnexosPermitidos)
            {
                await _js.InvokeVoidAsync("Swal.fire", new
                {
                    html = $@"
                    <div style='text-align: left; padding: 10px 5px; font-family: system-ui, -apple-system, sans-serif;'>
                        <!-- Cabecera con Icono -->
                        <div style='display: flex; align-items: center; gap: 14px; margin-bottom: 20px;'>
                            <div style='background-color: #fff1f2; color: #e11d48; width: 48px; height: 48px; border-radius: 12px; display: flex; align-items: center; justify-content: center; font-size: 24px; flex-shrink: 0; box-shadow: 0 2px 4px rgba(225, 29, 72, 0.1);'>
                                📑
                            </div>
                            <div>
                                <h3 style='margin: 0; font-size: 18px; font-weight: 700; color: #1e293b;'>Límite de Anexos Alcanzado</h3>
                                <p style='margin: 2px 0 0 0; font-size: 13px; color: #64748b;'>Gestión de Archivos Adjuntos</p>
                            </div>
                        </div>

                        <!-- Descripción -->
                        <p style='font-size: 14px; color: #475569; line-height: 1.5; margin-bottom: 20px;'>
                            Ha excedido la cantidad máxima de archivos adjuntos permitidos para este trámite.
                        </p>

                        <!-- Caja de Resumen -->
                        <div style='background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 12px; padding: 16px; margin-bottom: 20px;'>
                            <div style='display: flex; justify-content: space-between; align-items: center; font-size: 14px;'>
                                <span style='color: #64748b;'>Máximo permitido:</span>
                                <span style='font-weight: 700; color: #1e293b; font-size: 16px;'>{maxAnexosPermitidos} archivos</span>
                            </div>
                        </div>

                        <!-- Nota de ayuda -->
                        <div style='display: flex; align-items: center; gap: 8px; font-size: 12px; color: #be123c; background: #fff1f2; padding: 10px 12px; border-radius: 8px;'>
                            <span>⚠️</span>
                            <span>Por favor, verifique su selección antes de continuar.</span>
                        </div>
                    </div>
                ",
                    showConfirmButton = true,
                    confirmButtonText = "Entendido",
                    confirmButtonColor = "#1e293b",
                    backdrop = "rgba(15, 23, 42, 0.5)",
                    customClass = new
                    {
                        popup = "rounded-2xl shadow-xl border border-slate-100"
                    }
                });
                return archivosActuales;
            }

            // 2. Obtener peso máximo para anexos (por defecto 20 MB o desde configuración)
            int maxMbAnexo = _configuration.GetValue<int>("ConfiguracionAdjuntos:PesoMaximoAnexoMB");
            if (maxMbAnexo <= 0) maxMbAnexo = 20;

            long maxFileSize = maxMbAnexo * 1024 * 1024L;
            var listaProcesada = new List<ArchivoAdjunto>(archivosActuales);

            // Variable para llevar la secuencia basada en los archivos que ya existen más los nuevos
            int contadorAnexos = listaProcesada.Count;

            foreach (var archivo in archivosSeleccionados)
            {
                // 2. VALIDACIÓN DE DUPLICADOS (Por Nombre y Tamaño exacto)
                bool yaExiste = listaProcesada.Any(a => a.Nombre.Equals(archivo.Name, StringComparison.OrdinalIgnoreCase) &&
                                                    a.Tamano == $"{Math.Round((double)archivo.Size / 1024, 2)} KB");
                if (yaExiste)
                {
                    await _js.InvokeVoidAsync("Swal.fire", new
                    {
                        html = $@"
                            <div style='text-align: left; padding: 10px 5px; font-family: system-ui, -apple-system, sans-serif;'>
                                <!-- Cabecera con Icono -->
                                <div style='display: flex; align-items: center; gap: 14px; margin-bottom: 20px;'>
                                    <div style='background-color: #fefce8; color: #ca8a04; width: 48px; height: 48px; border-radius: 12px; display: flex; align-items: center; justify-content: center; font-size: 24px; flex-shrink: 0; box-shadow: 0 2px 4px rgba(202, 138, 4, 0.1);'>
                                        ⚠️
                                    </div>
                                    <div>
                                        <h3 style='margin: 0; font-size: 18px; font-weight: 700; color: #1e293b;'>Archivo Duplicado</h3>
                                        <p style='margin: 2px 0 0 0; font-size: 13px; color: #64748b;'>Control de Anexos</p>
                                    </div>
                                </div>

                                <!-- Descripción -->
                                <p style='font-size: 14px; color: #475569; line-height: 1.5; margin-bottom: 20px;'>
                                    El documento que intenta adjuntar ya fue registrado previamente en la lista de anexos de este trámite.
                                </p>

                                <!-- Tarjeta de Archivo Duplicado -->
                                <div style='background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 12px; padding: 16px; margin-bottom: 20px;'>
                                    <div style='display: flex; justify-content: space-between; align-items: center; font-size: 14px;'>
                                        <span style='color: #64748b;'>Archivo existente:</span>
                                        <span style='font-weight: 600; color: #0f172a; max-width: 200px; white-space: nowrap; overflow: hidden; text-overflow: ellipsis;' title='{archivo.Name}'>{archivo.Name}</span>
                                    </div>
                                </div>

                                <!-- Nota de ayuda -->
                                <div style='display: flex; align-items: center; gap: 8px; font-size: 12px; color: #a16207; background: #fefce8; padding: 10px 12px; border-radius: 8px;'>
                                    <span>💡</span>
                                    <span>No es necesario adjuntar el mismo archivo dos veces. Verifique su lista.</span>
                                </div>
                            </div>
                        ",
                        showConfirmButton = true,
                        confirmButtonText = "Entendido",
                        confirmButtonColor = "#1e293b",
                        backdrop = "rgba(15, 23, 42, 0.5)",
                        customClass = new
                        {
                            popup = "rounded-2xl shadow-xl border border-slate-100"
                        }
                    });
                    continue;
                }

                // 3. Validar extensión PDF
                if (!archivo.Name.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    await _js.InvokeVoidAsync("Swal.fire", new
                    {
                        html = $@"
                            <div style='text-align: left; padding: 10px 5px; font-family: system-ui, -apple-system, sans-serif;'>
                                <!-- Cabecera con Icono -->
                                <div style='display: flex; align-items: center; gap: 14px; margin-bottom: 20px;'>
                                    <div style='background-color: #fef2f2; color: #dc2626; width: 48px; height: 48px; border-radius: 12px; display: flex; align-items: center; justify-content: center; font-size: 24px; flex-shrink: 0; box-shadow: 0 2px 4px rgba(220, 38, 38, 0.1);'>
                                        📄
                                    </div>
                                    <div>
                                        <h3 style='margin: 0; font-size: 18px; font-weight: 700; color: #1e293b;'>Formato No Permitido</h3>
                                        <p style='margin: 2px 0 0 0; font-size: 13px; color: #64748b;'>Validación de Documento</p>
                                    </div>
                                </div>

                                <!-- Descripción -->
                                <p style='font-size: 14px; color: #475569; line-height: 1.5; margin-bottom: 20px;'>
                                    El archivo que intenta adjuntar no corresponde a un formato válido para el trámite.
                                </p>

                                <!-- Tarjeta de Archivo Rechazado -->
                                <div style='background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 12px; padding: 16px; margin-bottom: 20px;'>
                                    <div style='display: flex; align-items: center; justify-content: space-between; font-size: 14px;'>
                                        <span style='color: #64748b;'>Archivo seleccionado:</span>
                                        <span style='font-weight: 600; color: #0f172a; max-width: 200px; white-space: nowrap; overflow: hidden; text-overflow: ellipsis;' title='{archivo.Name}'>{archivo.Name}</span>
                                    </div>
                                    <div style='display: flex; justify-content: space-between; align-items: center; font-size: 14px; border-top: 1px dashed #cbd5e1; margin-top: 10px; padding-top: 10px;'>
                                        <span style='color: #64748b;'>Formato requerido:</span>
                                        <span style='font-weight: 600; color: #dc2626; background: #fee2e2; padding: 2px 8px; border-radius: 6px;'>Solo PDF (.pdf)</span>
                                    </div>
                                </div>

                                <!-- Nota de ayuda -->
                                <div style='display: flex; align-items: center; gap: 8px; font-size: 12px; color: #b91c1c; background: #fef2f2; padding: 10px 12px; border-radius: 8px;'>
                                    <span>⚠️</span>
                                    <span>Convierta su documento a PDF antes de volver a adjuntarlo.</span>
                                </div>
                            </div>
                        ",
                        showConfirmButton = true,
                        confirmButtonText = "Entendido",
                        confirmButtonColor = "#1e293b",
                        backdrop = "rgba(15, 23, 42, 0.5)",
                        customClass = new
                        {
                            popup = "rounded-2xl shadow-xl border border-slate-100"
                        }
                    });
                    continue;
                }

                // 4. Validar archivo vacío
                if (archivo.Size <= 0)
                {
                    await _js.InvokeVoidAsync("Swal.fire", "Archivo vacío", $"El archivo '{archivo.Name}' no contiene información.", "warning");
                    continue;
                }

                // 5. Validar peso de cada archivo
                if (archivo.Size > maxFileSize)
                {
                    double tamanoActualMb = Math.Round((double)archivo.Size / (1024 * 1024), 2);
                    await _js.InvokeVoidAsync("Swal.fire", new
                    {
                        html = $@"
                                <div style='text-align: left; padding: 10px 5px; font-family: system-ui, -apple-system, sans-serif;'>
                                    <!-- Cabecera con Icono -->
                                    <div style='display: flex; align-items: center; gap: 14px; margin-bottom: 20px;'>
                                        <div style='background-color: #fef3c7; color: #d97706; width: 48px; height: 48px; border-radius: 12px; display: flex; align-items: center; justify-content: center; font-size: 24px; flex-shrink: 0; box-shadow: 0 2px 4px rgba(217, 119, 6, 0.1);'>
                                            ⚠️
                                        </div>
                                        <div>
                                            <h3 style='margin: 0; font-size: 18px; font-weight: 700; color: #1e293b;'>Archivo Demasiado Pesado</h3>
                                            <p style='margin: 2px 0 0 0; font-size: 13px; color: #64748b;'>Control de Capacidad de Anexos</p>
                                        </div>
                                    </div>

                                    <!-- Descripción -->
                                    <p style='font-size: 14px; color: #475569; line-height: 1.5; margin-bottom: 20px;'>
                                        El archivo que intenta adjuntar excede el peso límite establecido para los anexos de este trámite.
                                    </p>

                                    <!-- Tarjeta de Detalles del Archivo -->
                                    <div style='background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 12px; padding: 16px; margin-bottom: 20px;'>
                                        <div style='display: flex; justify-content: space-between; align-items: center; font-size: 14px; margin-bottom: 10px;'>
                                            <span style='color: #64748b;'>Archivo:</span>
                                            <span style='font-weight: 600; color: #0f172a; max-width: 200px; white-space: nowrap; overflow: hidden; text-overflow: ellipsis;' title='{archivo.Name}'>{archivo.Name}</span>
                                        </div>
                                        <div style='display: flex; justify-content: space-between; align-items: center; font-size: 14px; margin-bottom: 10px;'>
                                            <span style='color: #64748b;'>Peso actual:</span>
                                            <span style='font-weight: 600; color: #ef4444; background: #fee2e2; padding: 2px 8px; border-radius: 6px;'>{tamanoActualMb} MB</span>
                                        </div>
                                        <div style='display: flex; justify-content: space-between; align-items: center; font-size: 14px; border-top: 1px dashed #cbd5e1; padding-top: 10px;'>
                                            <span style='color: #64748b;'>Límite máximo permitido:</span>
                                            <span style='font-weight: 600; color: #0f172a;'>{maxMbAnexo} MB</span>
                                        </div>
                                    </div>

                                    <!-- Nota de ayuda -->
                                    <div style='display: flex; align-items: center; gap: 8px; font-size: 12px; color: #b45309; background: #fef3c7; padding: 10px 12px; border-radius: 8px;'>
                                        <span>💡</span>
                                        <span>Optimice o comprima el documento antes de volver a adjuntarlo.</span>
                                    </div>
                                </div>
                            ",
                        showConfirmButton = true,
                        confirmButtonText = "Entendido",
                        confirmButtonColor = "#1e293b",
                        backdrop = "rgba(15, 23, 42, 0.5)",
                        customClass = new
                        {
                            popup = "rounded-2xl shadow-xl border border-slate-100"
                        }
                    }); continue;
            }

            try
            {
                await using var stream = archivo.OpenReadStream(maxFileSize);
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);

                if (ms.Length <= 0)
                {
                    Console.WriteLine($"[Anexo vacío después de lectura] Archivo: {archivo.Name}");
                    await _js.InvokeVoidAsync("Swal.fire", "Error de lectura", $"El archivo '{archivo.Name}' no pudo ser leído correctamente.", "error");
                    continue;
                }

                if (ms.Length != archivo.Size)
                {
                    Console.WriteLine($"[Carga incompleta de anexo] Archivo: {archivo.Name} | Esperado: {archivo.Size} bytes | Recibido: {ms.Length} bytes");
                    await _js.InvokeVoidAsync(
                        "Swal.fire",
                        "Carga incompleta",
                        $"El archivo '{archivo.Name}' no se cargó completamente. Tamaño esperado: {archivo.Size} bytes. Tamaño recibido: {ms.Length} bytes.",
                        "error");
                    continue;
                }

                // Incrementamos el contador para generar el nombre secuencial
                contadorAnexos++;
                string nombreSecuencial = $"ANEXO {contadorAnexos}.pdf";

                listaProcesada.Add(new ArchivoAdjunto
                {
                    Nombre = nombreSecuencial, // Se asigna "ANEXO 1.pdf", "ANEXO 2.pdf", etc.
                    Tipo = archivo.ContentType,
                    Tamano = $"{Math.Round((double)archivo.Size / 1024, 2)} KB",
                    Contenido = ms.ToArray()
                });

                Console.WriteLine($"[Anexo cargado correctamente] Original: {archivo.Name} -> Asignado: {nombreSecuencial} | Tamaño: {archivo.Size} bytes");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"[IOException ProcesarAnexo] Archivo: {archivo.Name} | {ex}");
                await _js.InvokeVoidAsync("Swal.fire", "Error de lectura", $"No se pudo leer completamente el archivo '{archivo.Name}'. {ex.Message}", "error");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error ProcesarAnexo] Archivo: {archivo.Name} | {ex}");
                await _js.InvokeVoidAsync("Swal.fire", "Error", $"No se pudo cargar el archivo '{archivo.Name}': {ex.Message}", "error");
            }
        }

    return listaProcesada;
}




}
}
