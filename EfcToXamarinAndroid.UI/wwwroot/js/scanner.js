window.qrScanner = {
    video: null,
    canvas: null,
    context: null,
    stream: null,
    scanning: false,
    dotNetHelper: null,

    start: async function (dotNetHelper, videoElement, canvasElement) {
        this.dotNetHelper = dotNetHelper;
        this.video = videoElement;
        this.canvas = canvasElement;
        this.context = this.canvas.getContext("2d", { willReadFrequently: true });
        this.scanning = true;

        try {
            this.stream = await navigator.mediaDevices.getUserMedia({ video: { facingMode: "environment" } });
            this.video.srcObject = this.stream;
            this.video.setAttribute("playsinline", true); // required to tell iOS safari we don't want fullscreen
            this.video.play();
            requestAnimationFrame(this.tick.bind(this));
            return true;
        } catch (err) {
            console.error(err);
            return false;
        }
    },

    stop: function () {
        this.scanning = false;
        if (this.video && this.video.srcObject) {
            this.video.pause();
            this.video.srcObject.getTracks().forEach(track => track.stop());
            this.video.srcObject = null;
        }
    },

    tick: function () {
        if (!this.scanning) {
            return;
        }

        if (this.video.readyState === this.video.HAVE_ENOUGH_DATA) {
            this.canvas.height = this.video.videoHeight;
            this.canvas.width = this.video.videoWidth;
            this.context.drawImage(this.video, 0, 0, this.canvas.width, this.canvas.height);
            
            var imageData = this.context.getImageData(0, 0, this.canvas.width, this.canvas.height);
            
            // Check if jsQR is loaded
            if (window.jsQR) {
                var code = jsQR(imageData.data, imageData.width, imageData.height, {
                    inversionAttempts: "dontInvert",
                });

                if (code && code.data && code.data.length > 0) {
                     this.dotNetHelper.invokeMethodAsync('OnQrCodeScanned', code.data);
                     // We don't stop automatically here, let the C# side decide to stop or keep scanning
                }
            }
        }

        requestAnimationFrame(this.tick.bind(this));
    }
};
