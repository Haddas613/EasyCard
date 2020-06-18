import i18n from '../i18n'

const primitives = {
    /**Common */
    required: (v) => !!v || i18n.t('Required'),

    maxLength: (max) => (v) => (!v || v.length <= max) || i18n.t('@MaxLength').replace("@max", max),
    stringLength: (min, max) => (v) => (!v || (v.length >= min && v.length <= max)) || i18n.t('@StringLength').replace("@min", min).replace("@max", max),

    inRange: (min, max) => (v) => (v >= min && v <= max) || i18n.t('@InRange').replace("@min", min).replace("@max", max),
    inRangeFlat: (min, max) => (v) => (v >= min && v <= max) || `${min}-${max}`,

    expired: (allowedTo) => (v) => (v >= allowedTo) || i18n.t('Expired'),

    biggerThan: (min) => (v) => (v > min) || i18n.t('@BiggerThan').replace('@min', min),

    email: (v) => {
        if(!v)
            return true;
        let regex = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
        return regex.test(v) || i18n.t('EmailMustBeValid');
    },
};

const complex = {
    cvv: [primitives.required, v => /^[0-9]{3,5}$/.test(v) || i18n.t('Invalid')],
    month: [primitives.required, primitives.inRangeFlat(1, 12)],
}

const special = {
    israeliNationalId: (v) => {
        if(!v)
            return true;
        
        var id = String(v).trim();
        if (id.length > 9 || id.length < 5 || isNaN(id)) 
            return i18n.t('IsraeliNationalIdMustBeValid');

        // Pad string with zeros up to 9 digits
        id = id.length < 9 ? ("00000000" + id).slice(-9) : id;

        return (Array
            .from(id, Number)
            .reduce((counter, digit, i) => {
                const step = digit * ((i % 2) + 1);
                return counter + (step > 9 ? step - 9 : step);
            }) % 10 === 0) || i18n.t('IsraeliNationalIdMustBeValid');
    }
}

/**
 * Common validation rules to be used across the app.
 * complex: contains completed rules for particular input type (cvv/month for example);
 * primitives: contains primitives functions
 * special: contains special cases (like Israeli national id)
 */
export default {
    complex: complex,
    primitives: primitives,
    special: special
}