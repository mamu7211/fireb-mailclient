window.clickElement = function (element) {
    if (element) element.click();
};

window.cropperInterop = {
    _instance: null,

    init: function (imageElement) {
        this.destroy();
        return new Promise(function (resolve, reject) {
            try {
                var cropper = new Cropper(imageElement, {
                    aspectRatio: 1,
                    viewMode: 1,
                    dragMode: 'move',
                    autoCropArea: 1,
                    cropBoxResizable: true,
                    cropBoxMovable: true,
                    guides: false,
                    center: false,
                    highlight: false,
                    background: false,
                    ready: function () {
                        window.cropperInterop._instance = cropper;
                        resolve(true);
                    }
                });
            } catch (e) {
                reject(e);
            }
        });
    },

    destroy: function () {
        if (this._instance) {
            this._instance.destroy();
            this._instance = null;
        }
    },

    getCroppedImageBase64: function (width, height) {
        if (!this._instance) return null;
        var canvas = this._instance.getCroppedCanvas({
            width: width,
            height: height,
            imageSmoothingEnabled: true,
            imageSmoothingQuality: 'high'
        });
        if (!canvas) return null;
        return canvas.toDataURL('image/png').split(',')[1];
    }
};
