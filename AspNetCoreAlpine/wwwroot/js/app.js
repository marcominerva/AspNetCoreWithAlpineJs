function init() {
    Alpine.data("app", () => ({
        open: false,
        message: '',

        toggle: function () {
            this.open = !this.open;
            this.message = '';
        }
    }));
}