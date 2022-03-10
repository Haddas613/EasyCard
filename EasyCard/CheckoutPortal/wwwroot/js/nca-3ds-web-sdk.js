"use strict";

function _instanceof(left, right) { if (right != null && typeof Symbol !== "undefined" && right[Symbol.hasInstance]) { return !!right[Symbol.hasInstance](left); } else { return left instanceof right; } }

/**
 * Returns the EMVCo-specified  window sizes depending on the challengeWindowSize value.
 *
 * @param challengeWindowSize - EMVCo-specified window size
 * @returns {string[]} - window size
 */
var getWindowSize = function getWindowSize() {
    var challengeWindowSize = arguments.length > 0 && arguments[0] !== undefined ? arguments[0] : '05';

    switch (challengeWindowSize) {
        case '01':
            return ['250px', '400px'];

        case '02':
            return ['390px', '400px'];

        case '03':
            return ['500px', '600px'];

        case '04':
            return ['600px', '400px'];

        case '05':
            return ['100%', '100%'];

        default:
            throw Error("Selected window size ".concat(challengeWindowSize, " is not supported"));
    }
};
/**
 * Creates a form element with one input field and sets the input value.
 *
 * @param formName - the name of the form element
 * @param formAction - the endpoint where the form will be submitted
 * @param formTarget - the iFrame name where the form will be appended to
 * @param inputName - the name of the field
 * @param inputValue - the value of the field
 * @throws {Error} - throws an error if there is a validation error
 * @returns {HTMLFormElement} - the generated form element
 */


var createForm = function createForm(formName, formAction, formTarget, inputName, inputValue) {
    if (!formName || !formAction || !formTarget || !inputName || !inputValue) {
        throw Error('All fields must be present');
    }

    var form = document.createElement('form');
    form.name = formName;
    form.action = formAction;
    form.method = 'POST';
    form.target = formTarget;
    form.enctype = "application/x-www-form-urlencoded";
    var input = document.createElement('input');
    input.name = inputName;
    input.value = inputValue;
    form.appendChild(input);
    form.style.display = 'none';
    console.log('form created');
    return form;
};
/**
 * Creates an iframe component and attaches it to the provided container.
 *
 * @param container - HTML element to attach the iframe
 * @param name - name of the iframe container. It will be used when attaching the form element
 * @param id - id of the iframe container
 * @param width - width of the container. Default is 0.
 * @param height - height of the container. Default is 0.
 * @param onLoadCallback - callback that will be executed when the frame loads. This is optional
 * @returns {HTMLIFrameElement} generated iframe
 */


var createIFrame = function createIFrame(container, name, id) {
    var width = arguments.length > 3 && arguments[3] !== undefined ? arguments[3] : '0';
    var height = arguments.length > 4 && arguments[4] !== undefined ? arguments[4] : '0';
    var onLoadCallback = arguments.length > 5 ? arguments[5] : undefined;

    if (!container || !name || !id) {
        throw Error('Not all required fields have value');
    }

    if (_instanceof(container, HTMLElement) === false) {
        throw Error('Container must be a HTML element');
    }

    var iframe = document.createElement('iframe');
    iframe.width = width;
    iframe.height = height;
    iframe.name = name;
    iframe.setAttribute('id', id);
    iframe.setAttribute('frameborder', '0');
    iframe.setAttribute('border', '0');
    iframe.setAttribute('style', 'overflow:hidden; position:absolute');

    if (onLoadCallback && typeof onLoadCallback === 'function') {
        if (iframe.attachEvent) {
            iframe.attachEvent('onload', onLoadCallback);
        } else {
            iframe.onload = onLoadCallback;
        }
    }

    container.appendChild(iframe);
    return iframe;
};

var init3DSMethod = function init3DSMethod(threeDSMethodUrl, threeDSMethodData, container) {
    if (!threeDSMethodUrl || !threeDSMethodData || !container) {
        throw Error('Not all fields have value');
    }

    if (_instanceof(container, HTMLIFrameElement) === false) {
        throw Error('Container is not an iFrame element');
    }

    if (!container.name) {
        throw Error('Container must have a name attribute');
    }

    var html = document.createElement('html');
    var body = document.createElement('body');
    var form = createForm('threeDSMethodForm', threeDSMethodUrl, container.name, "threeDSMethodData", threeDSMethodData);
    body.appendChild(form);
    html.appendChild(body);
    container.appendChild(html);
    container.style.display = 'none';
    form.submit();
    return container;
};

var init3DSChallengeRequest = function init3DSChallengeRequest(acsUrl, creqData, container) {
    if (!acsUrl || !creqData || !container) {
        throw Error('Not all required fields have value');
    }

    if (_instanceof(container, HTMLIFrameElement) === false) {
        throw Error('Container is not of type iframe');
    }

    if (!container.name) {
        throw Error('Container must have a name attribute');
    }

    var html = document.createElement('html');
    var body = document.createElement('body');
    var form = createForm('challengeRequestForm', acsUrl, container.name, "creq", creqData);
    body.appendChild(form);
    html.appendChild(body);
    container.appendChild(html);
    form.submit();
    return container;
};

var createIframeAndInit3DSMethod = function createIframeAndInit3DSMethod(threeDSMethodUrl, threeDSMethodData) {
    var frameName = arguments.length > 2 && arguments[2] !== undefined ? arguments[2] : 'threeDSMethodIFrame';
    var rootContainer = arguments.length > 3 && arguments[3] !== undefined ? arguments[3] : document.body;
    var onFrameLoadCallback = arguments.length > 4 ? arguments[4] : undefined;
    var iFrame = createIFrame(rootContainer, frameName, 'threeDSMethodIframe', '0', '0', onFrameLoadCallback);
    init3DSMethod(threeDSMethodUrl, threeDSMethodData, iFrame);
    //sleep(7000); 
    return iFrame;
};

function sleep(millisecond) {
    const date = Date.now();
    let currentDate = null;
    do {
        currentDate = Date.now();
    } while (currentDate - date < millisecond)
}

var createIFrameAndInit3DSChallengeRequest = function createIFrameAndInit3DSChallengeRequest(acsUrl, creqData) {
    var challengeWindowSize = arguments.length > 2 && arguments[2] !== undefined ? arguments[2] : '05';
    var frameName = arguments.length > 3 && arguments[3] !== undefined ? arguments[3] : "threeDSCReqIFrame";
    var rootContainer = arguments.length > 4 && arguments[4] !== undefined ? arguments[4] : document.body;
    var onFrameLoadCallback = arguments.length > 5 ? arguments[5] : undefined;
    var windowSize = getWindowSize(challengeWindowSize);
    var iFrame = createIFrame(rootContainer, frameName, 'threeDSCReqIframe', windowSize[0], windowSize[1], onFrameLoadCallback);
    init3DSChallengeRequest(acsUrl, creqData, iFrame);
    return iFrame;
};
/**
 * Attach all methods to window.
 */


var nca3DSWebSDK = {}; // START SNIPPET: websdk-documentation

/**
 * Creates an iframe, attach it to the rootContainer and submit 3DS Method form.
 *
 * @param threeDSMethodUrl - a FQDN endpoint to submit the 3DS Method request
 * @param threeDSMethodData - Base64-encoded 3DS Method Data value
 * @param frameName - name of the frame container. if not set it will be set to 'threeDSMethodIFrame'
 * @param rootContainer - the container where the iframe will be attached to.
 *                        If not set defaults to the JavaScript document.body object
 * @param onFrameLoadCallback - callback function attached to the iframe.onload event
 * @throws {Error} - throws error if there is a validation error
 * @returns {HTMLIFrameElement} - returns the generated iframe element
 */

nca3DSWebSDK.createIframeAndInit3DSMethod = createIframeAndInit3DSMethod;
/**
 * Initiates a 3DS Method request and submits the form the the 3DS Method URL. It will automatically hide the container
 * when initiating a 3DS Method request.
 *
 * @param threeDSMethodUrl - a FQDN endpoint to submit the 3DS Method request
 * @param threeDSMethodData - Base64-encoded 3DS Method Data value.
 * @param container - the iframe container where the form will be attached to. The container must have the 'name'
 *                    attribute set
 * @throws {Error} - throws error if there is a validation error
 * @returns {HTMLIFrameElement} - the container
 */

nca3DSWebSDK.init3DSMethod = init3DSMethod;
/**
 * Initiates a 3DS Challenge request and submits the form the the ACS URL.
 *
 * @param acsUrl - the FQDN URL to submit the Challenge Request
 * @param creqData - Base64-encoded Challenge Request
 * @param container - the iframe container where the form will be attached to. The container must have the 'name'
 *                    attribute set
 * @throws {Error} - throws error if there is a validation error
 * @returns {HTMLIFrameElement} - the container
 */

nca3DSWebSDK.init3DSChallengeRequest = init3DSChallengeRequest;
/**
 * Creates an iframe, attach it to the rootContainer and submits 3DS Challenge Request.
 * @param acsUrl - the FQDN URL to submit the Challenge Request
 * @param creqData - Base64-encoded Challenge Request
 * @param challengeWindowSize - EMVCo assigned window size.
 *                              '01' -> 250px x 400px,
 *                              '02' -> 390px x 400px,
 *                              '03' -> 500px x 600px,
 *                              '04' -> 600px x 400px,
 *                              '05' -> Full screen, or full container content
 * @param frameName - name of the frame container. if not set it will be set to 'threeDSCReqIFrame'
 * @param rootContainer - the container where the iframe will be attached to.
 *                        If not set defaults to the JavaScript document.body object
 * @param onFrameLoadCallback - callback function attached to the iframe.onload event
 * @throws {Error} - throws error if there is a validation error
 * @returns {HTMLIFrameElement} - returns the generated iframe element
 */

nca3DSWebSDK.createIFrameAndInit3DSChallengeRequest = createIFrameAndInit3DSChallengeRequest;
window.nca3DSWebSDK = nca3DSWebSDK; // END SNIPPET: websdk-documentation