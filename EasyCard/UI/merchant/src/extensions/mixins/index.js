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
            return terminal && terminal.integrations && terminal.integrations[integration];
        },
        $apiSuccess: function (operation){
            return operation && operation.status == "success";
        },
        $showDeleted: function(boolean){
            /**
             * OnlyActive = 0,
             * OnlyDeleted = 1,
             * All = 2 (not supported)
             */
            return boolean ? 1 : 0;
        }
    }
};

export default mixins;