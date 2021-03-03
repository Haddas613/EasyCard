const mixins = {
    methods: {
        $copyToClipboard: function (text, showToasted = true) {
            const self = this;
            this.$copyText(text).then(function (e) {
                self.$toasted.show(self.$t("CopiedToClipboard"), { type: 'success' })
            }, function (e) {
                self.$toasted.show(self.$t("CanNotCopyToClipboard"));
                console.log(`Error while copying to clipboard: ${e}`);
            });
        }
    }
};

export default mixins;