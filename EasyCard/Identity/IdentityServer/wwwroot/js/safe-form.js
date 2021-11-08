$(document).ready(function () {
    $("#submit-btn").click(function (e) {
        if (e.target.dataset["sending"] == "true") {
            e.preventDefault();          
        }
        e.target.dataset["sending"] = true;
        if (!this.timeout) {
            this.timeout = setTimeout(() => e.target.dataset["sending"] = false, 3000);
        } else {
            clearTimeout(this.timeout);
            this.timeout = setTimeout(() => e.target.dataset["sending"] = false, 3000);
        }
    });
});