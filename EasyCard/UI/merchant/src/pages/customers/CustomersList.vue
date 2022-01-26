<template>
  <v-flex width="100%">
    <customers-list
      :show-previously-charged="false"
      v-on:ok="customerClicked($event)"
      :filter-by-terminal="true"
      allow-show-deleted
      overview
    ></customers-list>
  </v-flex>
</template>

<script>
import { mapState } from "vuex";

export default {
  components: {
    CustomersList: () => import("../../components/customers/CustomersList")
  },
  data() {
    return {
      totalAmount: 0,
      dictionaries: {}
    };
  },
  computed: {
    ...mapState({
      showDeletedCustomers: state => state.ui.showDeletedCustomers
    })
  },
  methods: {
    customerClicked(customer) {
      this.$router.push({
        name: "Customer",
        params: { id: customer.consumerID }
      });
    },
    initThreeDotMenu() {
      this.$store.commit("ui/changeHeader", {
        value: {
          threeDotMenu: [
            {
              text: this.$t("CreateCustomer"),
              fn: () => this.$router.push({ name: "CreateCustomer" })
            },
            {
              text: this.showDeletedCustomers
                ? this.$t("ShowActive")
                : this.$t("ShowDeleted"),
              fn: () =>
                this.$store.commit(
                  "ui/setShowDeletedCustomers",
                  !this.showDeletedCustomers
                )
            }
          ],
          text: {
            translate: true,
            value: this.showDeletedCustomers ? "DeletedCustomers" : "Customers"
          }
        }
      });
    }
  },
  async mounted() {
    this.initThreeDotMenu();
  },
  watch: {
    showDeletedCustomers(newValue, oldValue) {
      this.initThreeDotMenu();
    }
  }
};
</script>