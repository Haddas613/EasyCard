jQuery(function ($) {

    $('.cc-number').payment('formatCardNumber');
    $('.cc-exp').payment('formatCardExpiry');
    $('.cc-cvc').payment('formatCardCVC');

    //$.fn.toggleInputError = function (erred) {
    //    this.parent('.form-group').find('.control-validation').toggleClass('.field-validation-error', erred);
    //    return this;
    //};

    // TODO: validate before post

    //$('form').submit(function (e) {
    //    e.preventDefault();

    //    var cardType = $.payment.cardType($('.cc-number').val());
    //    $('.cc-number').toggleInputError(!$.payment.validateCardNumber($('.cc-number').val()));
    //    $('.cc-exp').toggleInputError(!$.payment.validateCardExpiry($('.cc-exp').payment('cardExpiryVal')));
    //    $('.cc-cvc').toggleInputError(!$.payment.validateCardCVC($('.cc-cvc').val(), cardType));
    //    $('.cc-brand').text(cardType);

    //    return true;
    //});

});