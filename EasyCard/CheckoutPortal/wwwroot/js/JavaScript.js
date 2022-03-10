$(function () {
    //@Model.PassedInit3DMethod=true;
    debugger;
    if ($("#threeDSMethodURLHidden") != undefined && $("#threeDSMethodURLHidden").val() != undefined) {
        var model = $("#ChargeModel").val();
        $.ajax({
            type: "POST",
            url: "/Home/Charge",
            data: JSON.parse(model),
            success: function () { window.location.reload(); }
        });
    }
})