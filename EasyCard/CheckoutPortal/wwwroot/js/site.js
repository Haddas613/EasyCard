jQuery(function ($) {

    $('.cc-number').payment('formatCardNumber');
    $('.cc-exp').payment('formatCardExpiry');
    $('.cc-cvc').payment('formatCardCVC');
});