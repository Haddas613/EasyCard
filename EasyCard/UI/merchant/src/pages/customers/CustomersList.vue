<template>
  <v-flex width="100%">
    <customers-list-component
      :show-previously-charged="false"
      v-on:ok="customerClicked($event)"
      :filter-by-terminal="true"
      allow-show-deleted
      overview
    ></customers-list-component>
  </v-flex>
</template>

<script>
import { mapState } from "vuex";

export default {
  name: "CustomersList",
  components: {
    CustomersListComponent: () => import("../../components/customers/CustomersList")
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
    },
    /** Header is initialized in mounted but since components are cached (keep-alive) it's required to
    manually update menu on route change to make sure header has correct value*/
    $route (to, from){
      /** only update header if we returned to the same (cached) page */
      if(to.meta.keepAlive == this.$options.name){
        this.initThreeDotMenu();
      }
    }
  }
};
</script>