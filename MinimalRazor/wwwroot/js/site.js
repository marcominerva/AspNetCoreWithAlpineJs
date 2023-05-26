function app() {
    Alpine.data("app", () => ({
        name: '',
        message: '',
        isBusy: false,

        sendName: async function() {

            this.isBusy = true;

            try {
                const response = await send(this.name);
                const content = await response.json();
                this.message = content.message;
            } catch (error) {
            }
            finally
            {
                this.isBusy = false;
            }
        }
    }));
}

async function send(name) {
    const request = { name: name };
    const response = await fetch('/api/greetings', {
        method: "POST",
        headers: {
            "Content-Type":"application/json"
        },
        body: JSON.stringify(request)
    });

    return response;
}