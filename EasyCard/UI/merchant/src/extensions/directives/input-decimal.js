//TODO: DELETE
const directive = {
    bind: function (el, binding, vnode) {
        // let split = vnode.data.model.expression.split(".");
        // let modelKey = split[split.length - 1];

        // if (!vnode.context.model[modelKey] || vnode.context.model[modelKey] === 0) {
        //     vnode.context.model[modelKey] = "0.00";
        // }

        // see if el is an input
        if (el.nodeName.toLowerCase() !== 'input') {
            el = el.querySelector('input');
        }

        el.addEventListener('keypress', evts.restrictDecimal);
        el.addEventListener('paste', evts.restrictDecimalPaste);
        el.addEventListener('input', evts.formatDecimal);
        // el.addEventListener('change', evts.formatDecimal);
    }
};

const evts = {
    restrictDecimal: function (e) {
        // Key event is for a browser shortcut
        if (e.metaKey || e.ctrlKey) { return true; }

        // If keycode is a space
        if (e.which === 32) { return false; }

        // If keycode is a special char (WebKit)
        if (e.which === 0) { return true; }

        // If char is a special char (Firefox)
        if (e.which < 33) { return true; }

        var input = String.fromCharCode(e.which);

        if (input === "." && (!e.target.value || e.target.value.indexOf(".") === -1)) { return true; }

        // Char is a number or a space
        return (!!/[\d\s]/.test(input)) ? true : e.preventDefault();
    },
    restrictDecimalPaste: function (e) {
        let value = e.clipboardData.getData("text").toString();
        try {
            let float = parseFloat(value);
            if (!float){ e.preventDefault(); }
        } catch{
            e.preventDefault();
            return false;
        }
    },
    formatDecimal: function(e, precision = 2){
        var target = e.currentTarget;
        // var value = target.value;
        //     if(value){
        //         value = Math.floor(parseFloat(value)).toFixed(2);
        //         target.value = value;
        //         console.log(value)
        //     }
        return setTimeout(function () {
            var value = target.value;
            if(value){
                value = parseFloat(value).toFixed(precision);
                target.value = value;
                console.log(value)
            }
        });
    }
}


export default directive;