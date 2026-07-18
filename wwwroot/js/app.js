navigator.serviceWorker.register('service-worker.js', { updateViaCache: 'none' });

// ===== Signature Pad (Canvas) =====
window.signaturePad = {
    _canvas: null,
    _ctx: null,
    _drawing: false,
    _hasContent: false,

    init: function (canvasId) {
        const canvas = document.getElementById(canvasId);
        if (!canvas) return;
        this._canvas = canvas;
        this._ctx = canvas.getContext('2d');
        this._hasContent = false;

        // Set canvas resolution to match display size
        const rect = canvas.getBoundingClientRect();
        canvas.width = rect.width;
        canvas.height = rect.height;

        this._ctx.strokeStyle = '#1a365d';
        this._ctx.lineWidth = 2.5;
        this._ctx.lineCap = 'round';
        this._ctx.lineJoin = 'round';

        // Clear
        this._ctx.fillStyle = '#ffffff';
        this._ctx.fillRect(0, 0, canvas.width, canvas.height);

        const getPos = (e) => {
            const r = canvas.getBoundingClientRect();
            const touch = e.touches ? e.touches[0] : e;
            return { x: touch.clientX - r.left, y: touch.clientY - r.top };
        };

        const start = (e) => {
            e.preventDefault();
            this._drawing = true;
            const p = getPos(e);
            this._ctx.beginPath();
            this._ctx.moveTo(p.x, p.y);
        };

        const move = (e) => {
            if (!this._drawing) return;
            e.preventDefault();
            this._hasContent = true;
            const p = getPos(e);
            this._ctx.lineTo(p.x, p.y);
            this._ctx.stroke();
        };

        const end = (e) => {
            if (e) e.preventDefault();
            this._drawing = false;
        };

        canvas.addEventListener('mousedown', start);
        canvas.addEventListener('mousemove', move);
        canvas.addEventListener('mouseup', end);
        canvas.addEventListener('mouseleave', end);
        canvas.addEventListener('touchstart', start, { passive: false });
        canvas.addEventListener('touchmove', move, { passive: false });
        canvas.addEventListener('touchend', end);
    },

    clear: function () {
        if (!this._canvas) return;
        this._ctx.fillStyle = '#ffffff';
        this._ctx.fillRect(0, 0, this._canvas.width, this._canvas.height);
        this._hasContent = false;
    },

    isEmpty: function () {
        return !this._hasContent;
    },

    toDataUrl: function () {
        if (!this._canvas || !this._hasContent) return null;
        return this._canvas.toDataURL('image/png');
    },

    toBytes: function () {
        if (!this._canvas || !this._hasContent) return null;
        const dataUrl = this._canvas.toDataURL('image/png');
        const base64 = dataUrl.split(',')[1];
        const binary = atob(base64);
        const bytes = new Uint8Array(binary.length);
        for (let i = 0; i < binary.length; i++) {
            bytes[i] = binary.charCodeAt(i);
        }
        return bytes;
    }
};

// ===== Geolocation =====
window.getGeoLocation = function () {
    return new Promise((resolve, reject) => {
        if (!navigator.geolocation) {
            reject('Geolocation not supported');
            return;
        }
        navigator.geolocation.getCurrentPosition(
            (pos) => resolve({ lat: pos.coords.latitude, lng: pos.coords.longitude }),
            (err) => reject(err.message),
            { enableHighAccuracy: true, timeout: 10000 }
        );
    });
};
