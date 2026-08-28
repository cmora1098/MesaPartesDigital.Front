window.mpvPdf = {
    create: async function (streamRef) {
        console.log("[MPV JS] Iniciando creación del Blob PDF...");
        try {
            const arrayBuffer = await streamRef.arrayBuffer();
            console.log("[MPV JS] Bytes recibidos para preview:", arrayBuffer.byteLength);
            if (!arrayBuffer || arrayBuffer.byteLength === 0) {
                throw new Error("El stream recibido para el PDF está vacío.");
            }
            const blob = new Blob([arrayBuffer], { type: "application/pdf" });
            console.log("[MPV JS] Blob creado:", blob.size, "bytes");
            const url = URL.createObjectURL(blob);
            console.log("[MPV JS] ObjectURL creado:", url);
            return url;
        } catch (error) {
            console.error("[MPV JS] Error creando preview PDF:", error);
            throw error;
        }
    },
    revoke: function (url) {
        if (!url || !url.startsWith("blob:")) return;
        try {
            URL.revokeObjectURL(url);
            console.log("[MPV JS] ObjectURL liberado:", url);
        } catch (error) {
            console.warn("[MPV JS] No se pudo liberar ObjectURL:", error);
        }
    }
};
