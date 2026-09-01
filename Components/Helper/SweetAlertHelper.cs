using Microsoft.JSInterop;
using static MesaPartesDigital.Components.Pages.EditarTramite;

public static class SweetAlertHelper
{ 
    public class SweetAlertResult
    {
        public bool IsConfirmed { get; set; }
        public bool IsDenied { get; set; }
        public bool IsDismissed { get; set; }
    }
    public static async Task MostrarModalValidacionAsync(
    IJSRuntime jsRuntime,
    string titulo = "Campos Incompletos",
    string subtitulo = "Mesa de Partes Virtual",
    string mensajeError = "Existen campos obligatorios sin completar.",
    string etiquetaValidacion = "Dato requerido faltante",
    string mensajeSugerencia = "Revise los campos obligatorios marcados con asterisco (*) antes de guardar.",
    string icono = "⚠️")
    {
        string htmlContent = $@"
        <div style='text-align: left; padding: 10px 5px; font-family: system-ui, -apple-system, sans-serif;'>
            <div style='display: flex; align-items: center; gap: 14px; margin-bottom: 20px;'>
                <div style='background-color: #fef2f2; color: #dc2626; width: 48px; height: 48px; border-radius: 12px; display: flex; align-items: center; justify-content: center; font-size: 24px; flex-shrink: 0; box-shadow: 0 2px 4px rgba(220, 38, 38, 0.1);'>
                    {icono}
                </div>
                <div>
                    <h3 style='margin: 0; font-size: 18px; font-weight: 700; color: #1e293b;'>{titulo}</h3>
                    <p style='margin: 2px 0 0 0; font-size: 13px; color: #64748b;'>{subtitulo}</p>
                </div>
            </div>
            <p style='font-size: 14px; color: #475569; line-height: 1.5; margin-bottom: 20px;'>
                {mensajeError}
            </p>
            <div style='background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 12px; padding: 16px; margin-bottom: 20px;'>
                <div style='display: flex; justify-content: space-between; align-items: center; font-size: 14px;'>
                    <span style='color: #64748b;'>Validación:</span>
                    <span style='font-weight: 600; color: #dc2626; background: #fee2e2; padding: 2px 8px; border-radius: 6px;'>{etiquetaValidacion}</span>
                </div>
            </div>
            <div style='display: flex; align-items: center; gap: 8px; font-size: 12px; color: #b91c1c; background: #fef2f2; padding: 10px 12px; border-radius: 8px;'>
                <span>💡</span>
                <span>{mensajeSugerencia}</span>
            </div>
        </div>";

        await jsRuntime.InvokeVoidAsync("Swal.fire", new
        {
            html = htmlContent,
            showConfirmButton = true,
            confirmButtonText = "Entendido",
            confirmButtonColor = "#1e293b",
            backdrop = "rgba(15, 23, 42, 0.5)",
            customClass = new { popup = "rounded-2xl shadow-xl border border-slate-100" }
        });
    }

 
    private static async Task<T> MostrarModalBaseAsync<T>(
        IJSRuntime jsRuntime,
        string titulo,
        string subtitulo,
        string mensaje,
        string icono,
        string backgroundIcono,
        string colorIcono,
        string etiquetaAccion,
        string colorAccionTexto,
        string colorAccionFondo,
        string mensajeAlerta,
        string colorAlertaTexto,
        string colorAlertaFondo,
        object swalOptions)
    {
        string htmlCajaAccion = string.IsNullOrEmpty(etiquetaAccion) ? "" : $@"
            <div style='background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 12px; padding: 16px; margin-bottom: 16px;'>
                <div style='display: flex; justify-content: space-between; align-items: center; font-size: 14px;'>
                    <span style='color: #64748b;'>Estado de la sección:</span>
                    <span style='font-weight: 600; color: {colorAccionTexto}; background: {colorAccionFondo}; padding: 4px 10px; border-radius: 6px;'>{etiquetaAccion}</span>
                </div>
            </div>";

        string htmlAlertaInferior = string.IsNullOrEmpty(mensajeAlerta) ? "" : $@"
            <div style='display: flex; align-items: center; gap: 8px; font-size: 12px; color: {colorAlertaTexto}; background: {colorAlertaFondo}; padding: 12px 14px; border-radius: 8px;'>
                <span>📌</span>
                <span>{mensajeAlerta}</span>
            </div>";

        string htmlContent = $@"
        <div style='text-align: left; padding: 10px 5px; font-family: system-ui, -apple-system, sans-serif;'>
            <div style='display: flex; align-items: center; gap: 14px; margin-bottom: 20px;'>
                <div style='background-color: {backgroundIcono}; color: {colorIcono}; width: 48px; height: 48px; border-radius: 12px; display: flex; align-items: center; justify-content: center; font-size: 24px; flex-shrink: 0; box-shadow: 0 2px 4px rgba(0, 0, 0, 0.04);'>
                    {icono}
                </div>
                <div>
                    <h3 style='margin: 0; font-size: 18px; font-weight: 700; color: #1e293b;'>{titulo}</h3>
                    <p style='margin: 2px 0 0 0; font-size: 13px; color: #64748b;'>{subtitulo}</p>
                </div>
            </div>

            <p style='font-size: 14px; color: #475569; line-height: 1.5; margin-bottom: 20px;'>
                {mensaje}
            </p>

            {htmlCajaAccion}
            {htmlAlertaInferior}
        </div>";

        var updatedOptions = InlineHtml(swalOptions, htmlContent);

        if (typeof(T) == typeof(SweetAlertResult))
        {
            return await jsRuntime.InvokeAsync<T>("Swal.fire", new object[] { updatedOptions });
        }
        else
        {
            await jsRuntime.InvokeVoidAsync("Swal.fire", updatedOptions);
            return default!;
        }
    }

    private static object InlineHtml(object source, string html)
    {
        var type = source.GetType();
        var props = type.GetProperties();
        var dict = props.ToDictionary(p => p.Name, p => p.GetValue(source));
        dict["html"] = html;
        return dict;
    }

    public static async Task<SweetAlertResult> MostrarModalConfirmacionAsync(
        IJSRuntime jsRuntime, string titulo, string subtitulo, string mensaje,
        string textoBotonConfirmar = "Sí, actualizar", string etiquetaAccion = "Actualizar datos",
        string mensajeAlerta = "Verifique que la información sea correcta antes de confirmar.", string icono = "📁")
    {
        var options = new
        {
            html = "",
            showCancelButton = true,
            showConfirmButton = true,
            confirmButtonText = textoBotonConfirmar,
            cancelButtonText = "Cancelar",
            confirmButtonColor = "#1e293b",
            cancelButtonColor = "#64748b",
            backdrop = "rgba(15, 23, 42, 0.5)",
            customClass = new { popup = "rounded-2xl shadow-xl border border-slate-100" }
        };

        return await MostrarModalBaseAsync<SweetAlertResult>(
            jsRuntime, titulo, subtitulo, mensaje, icono,
            "#f0fdf4", "#16a34a",
            etiquetaAccion, "#16a34a", "#dcfce7",
            mensajeAlerta, "#15803d", "#f0fdf4",
            options);
    }

    public static async Task MostrarModalErrorAsync(
        IJSRuntime jsRuntime, string titulo, string subtitulo, string mensajeError,
        string textoBotonConfirmar = "Entendido", string etiquetaAccion = "",
        string mensajeAlerta = "", string icono = "❌")
    {
        var options = new
        {
            html = "",
            showConfirmButton = true,
            confirmButtonText = textoBotonConfirmar,
            confirmButtonColor = "#1e293b",
            backdrop = "rgba(15, 23, 42, 0.5)",
            customClass = new { popup = "rounded-2xl shadow-xl border border-slate-100" }
        };

        await MostrarModalBaseAsync<object>(
            jsRuntime, titulo, subtitulo, mensajeError, icono,
            "#fef2f2", "#dc2626",
            etiquetaAccion, "#dc2626", "#fee2e2",
            mensajeAlerta, "#b91c1c", "#fef2f2",
            options);
    }

    public static async Task<SweetAlertResult> MostrarModalExitoAccionAsync(
        IJSRuntime jsRuntime, string titulo, string subtitulo, string mensaje,
        string estadoSeccion = "Guardado parcial", string textoBotonConfirmar = "Enviar Trámite Subsanado",
        string textoBotonDenegar = "Seguir editando", string mensajeAlerta = "", string icono = "📁")
    {
        var options = new
        {
            html = "",
            showCancelButton = false,
            showConfirmButton = true,
            showDenyButton = true,
            confirmButtonText = textoBotonConfirmar,
            denyButtonText = textoBotonDenegar,
            confirmButtonColor = "#16a34a",
            denyButtonColor = "#64748b",
            backdrop = "rgba(15, 23, 42, 0.5)",
            customClass = new { popup = "rounded-2xl shadow-xl border border-slate-100" }
        };

        return await MostrarModalBaseAsync<SweetAlertResult>(
            jsRuntime, titulo, subtitulo, mensaje, icono,
            "#f0fdf4", "#16a34a",
            estadoSeccion, "#16a34a", "#dcfce7",
            mensajeAlerta, "#15803d", "#f0fdf4",
            options);
    }

    public static async Task<SweetAlertResult> MostrarModalExitoInformativoAsync(
    IJSRuntime jsRuntime, string titulo, string subtitulo, string mensaje,
    string textoBotonConfirmar = "Continuar", string etiquetaAccion = "",
    string mensajeAlerta = "", string icono = "📁")
    {
        var options = new
        {
            html = "",
            showCancelButton = false,
            showConfirmButton = true,
            showDenyButton = false, // Ocultamos el segundo botón definitivamente
            confirmButtonText = textoBotonConfirmar,
            confirmButtonColor = "#16a34a", // Verde de éxito
            backdrop = "rgba(15, 23, 42, 0.5)",
            customClass = new { popup = "rounded-2xl shadow-xl border border-slate-100" }
        };

        return await MostrarModalBaseAsync<SweetAlertResult>(
            jsRuntime, titulo, subtitulo, mensaje, icono,
            "#f0fdf4", "#16a34a",
            etiquetaAccion, "#16a34a", "#dcfce7",
            mensajeAlerta, "#15803d", "#f0fdf4",
            options);
    }

    public static async Task<SweetAlertResult> MostrarModalEliminacionAsync(
    IJSRuntime jsRuntime, string titulo, string subtitulo, string mensaje,
    string textoBotonConfirmar = "Sí, eliminar", string etiquetaAccion = "Dar de baja",
    string mensajeAlerta = "Verifique bien antes de proceder con la eliminación.", string icono = "🗑️")
    {
        var options = new
        {
            html = "",
            showCancelButton = true,
            showConfirmButton = true,
            confirmButtonText = textoBotonConfirmar,
            cancelButtonText = "Cancelar",
            confirmButtonColor = "#dc2626", // Color rojo de peligro
            cancelButtonColor = "#64748b",
            backdrop = "rgba(15, 23, 42, 0.5)",
            customClass = new { popup = "rounded-2xl shadow-xl border border-slate-100" }
        };

        return await MostrarModalBaseAsync<SweetAlertResult>(
            jsRuntime, titulo, subtitulo, mensaje, icono,
            "#fef2f2", "#dc2626", // Fondo e icono en tonos rojos
            etiquetaAccion, "#dc2626", "#fee2e2", // Etiqueta de acción roja
            mensajeAlerta, "#b91c1c", "#fef2f2", // Alerta inferior en tonos rojos
            options);
    }

}