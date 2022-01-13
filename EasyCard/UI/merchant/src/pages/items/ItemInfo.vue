<template>
  <v-flex>
    <v-card class="mx-2 my-2">
      <v-card-title class="py-2">
        <v-row no-gutters class="py-0">
          <v-col cols="9" class="d-flex">
            <span class="pt-2 ecdgray--text subtitle-2 text-uppercase">{{
              $t("ItemInformation")
            }}</span>
          </v-col>
          <v-col cols="3" class="d-flex justify-end">
            <v-btn
              text
              class="primary--text px-0"
              link
              :to="{ name: 'EditItem', params: { id: this.$route.params.id } }"
            >
              <v-icon left class="body-1">mdi-pencil-outline</v-icon>
              {{ $t("Edit") }}
            </v-btn>
          </v-col>
        </v-row>
      </v-card-title>
      <v-divider></v-divider>
      <v-card-text class="body-1 black--text">
        <div class="info-block">
          <p class="caption ecgray--text text--darken-2">{{ $t("ItemID") }}</p>
          <p class="caption">{{ model.itemID }}</p>
        </div>
        <div class="info-block">
          <p class="caption ecgray--text text--darken-2">{{ $t("Name") }}</p>
          <p>{{ model.itemName }}</p>
        </div>
        <div class="info-block">
          <p class="caption ecgray--text text--darken-2">{{ $t("Active") }}</p>
          <p v-if="model.active" class="success--text">{{ $t("Yes") }}</p>
          <p v-else class="error--text">{{ $t("No") }}</p>
        </div>
        <div class="info-block">
          <p class="caption ecgray--text text--darken-2">{{ $t("Price") }}</p>
          <p>
            <ec-money
              :amount="model.price"
              :currency="model.currency"
            ></ec-money>
          </p>
        </div>
        <div class="info-block">
          <p class="caption ecgray--text text--darken-2">
            {{ $t("ExternalReference") }}
          </p>
          <p>{{ model.externalReference || "-" }}</p>
        </div>
        <div class="info-block" v-if="model.billingDesktopRefNumber">
          <p class="caption ecgray--text text--darken-2">
            {{ $t("BillingDesktopRefNumber") }}
          </p>
          <p>{{ model.billingDesktopRefNumber }}</p>
        </div>
      </v-card-text>
    </v-card>
  </v-flex>
</template> 

<script>
import EcMoney from "../../components/ec/EcMoney";

export default {
  components: { EcMoney },
  props: {
    data: {
      type: Object,
      default: null,
      required: false,
    },
  },
  data() {
    return {
      model: {},
    };
  },
  methods: {
    createItem() {
      this.$router.push({ name: "CreateItem" });
    },
    async switchItem() {
      if(this.model.active){
        let result = await this.$api.items.deleteItem(this.$route.params.id);
        return this.$router.push({ name: "Items" });
      }
      else{
        let result = await this.$api.items.updateItem(this.$route.params.id, {
          ...this.model,
          active: true,
        });
        if (!this.$apiSuccess(result)) return;
        this.model.active = true;
      }
      this.initMenu();
    },
    initMenu() {
      this.$store.commit("ui/changeHeader", {
        value: {
          threeDotMenu: [
            {
              text: this.$t("CreateItem"),
              fn: this.createItem.bind(this),
            },
            {
              text: this.model.active ? this.$t("DeleteItem") : this.$t("RestoreItem"),
              fn: this.switchItem.bind(this),
            },
          ],
          text: { translate: false, value: this.model.itemName },
        },
      });
    },
  },
  async mounted() {
    if (this.data) {
      this.model = this.data;
      return;
    }

    this.model = await this.$api.items.getItem(this.$route.params.id);

    if (!this.model) {
      return this.$router.push({ name: "Items" });
    }

    this.initMenu();
  },
};
</script>

<style lang="scss" scoped>
p {
  margin-bottom: 0;
}
.info-block {
  padding-bottom: 1rem;
}
</style>