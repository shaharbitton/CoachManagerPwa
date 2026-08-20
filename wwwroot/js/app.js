navigator.serviceWorker.register('service-worker.js', { updateViaCache: 'none' });

// ===== Signature Pad (Canvas) =====
window.signaturePad = {
    _canvas: null,
    _ctx: null,
    _drawing: false,
    _hasContent: false,
    _listeners: [],

    init: function (canvasId) {
        const self = this;
        const canvas = document.getElementById(canvasId);
        if (!canvas) {
            console.warn('signaturePad.init: canvas not found:', canvasId);
            return;
        }

        // Remove old listeners if re-initializing
        self._listeners.forEach(([el, ev, fn, opts]) => el.removeEventListener(ev, fn, opts));
        self._listeners = [];

        self._canvas = canvas;
        self._ctx = canvas.getContext('2d');
        self._hasContent = false;
        self._drawing = false;

        // Set canvas resolution to match display size
        const rect = canvas.getBoundingClientRect();
        canvas.width = rect.width;
        canvas.height = rect.height;

        self._ctx.strokeStyle = '#1a365d';
        self._ctx.lineWidth = 2.5;
        self._ctx.lineCap = 'round';
        self._ctx.lineJoin = 'round';

        // Clear with white
        self._ctx.fillStyle = '#ffffff';
        self._ctx.fillRect(0, 0, canvas.width, canvas.height);

        const getPos = (e) => {
            const r = canvas.getBoundingClientRect();
            const touch = e.touches ? e.touches[0] : e;
            return { x: touch.clientX - r.left, y: touch.clientY - r.top };
        };

        const start = (e) => {
            e.preventDefault();
            e.stopPropagation();
            self._drawing = true;
            const p = getPos(e);
            self._ctx.beginPath();
            self._ctx.moveTo(p.x, p.y);
        };

        const move = (e) => {
            if (!self._drawing) return;
            e.preventDefault();
            e.stopPropagation();
            self._hasContent = true;
            const p = getPos(e);
            self._ctx.lineTo(p.x, p.y);
            self._ctx.stroke();
        };

        const end = (e) => {
            if (e) { e.preventDefault(); e.stopPropagation(); }
            self._drawing = false;
        };

        const add = (el, ev, fn, opts) => {
            el.addEventListener(ev, fn, opts);
            self._listeners.push([el, ev, fn, opts]);
        };

        add(canvas, 'mousedown', start, false);
        add(canvas, 'mousemove', move, false);
        add(canvas, 'mouseup', end, false);
        add(canvas, 'mouseleave', end, false);
        add(canvas, 'touchstart', start, { passive: false });
        add(canvas, 'touchmove', move, { passive: false });
        add(canvas, 'touchend', end, false);
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

// ===== Contract PDF =====
window.contractPdf = {
    // Render HTML string into a hidden iframe and trigger print (Save as PDF)
    printHtml: function (htmlContent) {
        const iframe = document.createElement('iframe');
        iframe.style.position = 'fixed';
        iframe.style.right = '-9999px';
        iframe.style.top = '-9999px';
        iframe.style.width = '210mm';
        iframe.style.height = '297mm';
        document.body.appendChild(iframe);

        iframe.contentDocument.open();
        iframe.contentDocument.write(htmlContent);
        iframe.contentDocument.close();

        iframe.onload = function () {
            iframe.contentWindow.focus();
            iframe.contentWindow.print();
            setTimeout(() => document.body.removeChild(iframe), 1000);
        };
    },

    // Download HTML as a file (fallback - saves .html which user can print to PDF)
    downloadHtml: function (htmlContent, fileName) {
        const blob = new Blob([htmlContent], { type: 'text/html;charset=utf-8' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = fileName || 'contract.html';
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
    },

    // Open HTML in new window for preview
    preview: function (htmlContent) {
        const win = window.open('', '_blank');
        win.document.write(htmlContent);
        win.document.close();
    }
};

// ===== File Download =====
window.downloadFile = function (fileName, content, mimeType) {
    const blob = new Blob([content], { type: mimeType || 'text/plain;charset=utf-8' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
};

// ===== Session Persistence =====
window.sessionStore = {
    save: function (key, value) { localStorage.setItem(key, value); },
    load: function (key) { return localStorage.getItem(key); },
    remove: function (key) { localStorage.removeItem(key); }
};
