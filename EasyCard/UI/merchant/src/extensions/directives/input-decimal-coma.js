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

        el.addEventListener('keydown', evts.formatDecimal);
        el.addEventListener('input', evts.restrictDecimal);
        // el.addEventListener('change', evts.formatDecimal);
    }
};

const evts = {
    restrictDecimal: function (e) {
        if(e.target.value.indexOf(".") > -1){
            e.preventDefault();
        }
    },
    formatDecimal: function(e){
        var target = e.currentTarget;
        var value = target.value;
        if(value){
            if(e.keyCode === 188) {
                if(e.target.value.indexOf(".") === -1) { target.value += "."; }
                e.preventDefault();
            }
        }
    }
}


export default directive;