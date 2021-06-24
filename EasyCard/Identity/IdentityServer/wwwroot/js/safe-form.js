$(document).ready(function () {
    $("#submit-btn").click(function (e) {
        if (e.target.dataset["sending"] == "true") {
            e.preventDefault();
        }
        e.target.dataset["sending"] = true;
    });
});