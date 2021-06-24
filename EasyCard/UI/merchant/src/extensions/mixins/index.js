import moment from "moment";

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
        },
        $formatDate: function (date) {
            return date ? moment(date).format("YYYY-MM-DD") : null;
        },
        $featureEnabled: function (terminal, feature) {
            return terminal && terminal.enabledFeatures.indexOf(feature) > -1;
        },
        $integrationAvailable: function (terminal, integration) {
            return terminal && terminal.integrations[integration];
        },
        $apiSuccess: function (operation){
            return operation && operation.status == "success";
        }
    }
};

export default mixins;