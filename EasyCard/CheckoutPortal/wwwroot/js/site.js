jQuery(function ($) {

    $('.cc-number').payment('formatCardNumber');
    $('.cc-exp').payment('formatCardExpiry');
    $('.cc-cvc').payment('formatCardCVC');
});

window.switchLoadingSpinner = function(show = false) {
    if (show) {
        document.getElementById("loading-spinner").hidden = false;
        document.getElementsByTagName("body")[0].classList.add("no-scroll");
    }
    else {
        document.getElementById("loading-spinner").hidden = true;
        document.getElementsByTagName("body")[0].classList.remove("no-scroll")
    }
}

toastr.options = {
    "closeButton": false,
    "newestOnTop": false,
    "progressBar": false,
    "positionClass": "toast-top-right",
    "preventDuplicates": false,
    "onclick": null,
    "timeOut": -1,
    "extendedTimeOut ": -1,
    "disableTimeOut": true,
    "tapToDismiss": false,
    "toastClass": "signalr-toast",
    "showEasing": "swing",
    "hideEasing": "linear",
    "showMethod": "fadeIn",
    "hideMethod": "fadeOut",
}