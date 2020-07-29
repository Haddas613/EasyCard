<template>
  <v-flex>
    <v-card class="mx-2 my-2">
      <v-card-title class="py-2">
        <v-row no-gutters class="py-0">
          <v-col cols="9" class="d-flex">
            <span class="pt-2 ecdgray--text subtitle-2 text-uppercase">{{$t('PersonalInformation')}}</span>
          </v-col>
          <v-col cols="3" class="d-flex justify-end">
            <v-btn
              text
              class="primary--text px-0"
              link
              :to="{name: 'EditCustomer', params: { id: this.$route.params.id}}"
            >
              <v-icon left class="body-1">mdi-pencil-outline</v-icon>
              {{$t('Edit')}}
            </v-btn>
          </v-col>
        </v-row>
      </v-card-title>
      <v-divider></v-divider>
      <v-card-text class="body-1 black--text">
        <div class="info-block">
          <p class="caption ecgray--text text--darken-2">{{$t('Name')}}</p>
          <p>{{model.consumerName}}</p>
        </div>
        <div class="info-block">
          <p class="caption ecgray--text text--darken-2">{{$t('Phone')}}</p>
          <p class="primary--text">{{model.consumerPhone}}</p>
        </div>
        <div class="info-block">
          <p class="caption ecgray--text text--darken-2">{{$t('Email')}}</p>
          <p class="primary--text">{{model.consumerEmail}}</p>
        </div>
        <div class="info-block">
          <p class="caption ecgray--text text--darken-2">{{$t('Address')}}</p>
          <p>{{model.consumerAddress}}</p>
        </div>
      </v-card-text>
    </v-card>
  </v-flex>
</template> 

<script>
export default {
  props: {
    data: {
      type: Object,
      default: null,
      required: false
    }
  },
  data() {
    return {
      model: {}
    };
  },
  methods: {
    createCustomer(){
      console.log("TODO: Create customer")
    },
    deleteCustomer(){
      console.log("TODO: delete customer")
    }
  },
  async mounted() {
    if (this.data) {
      this.model = this.data;
      return;
    }

    this.model = await this.$api.consumers.getConsumer(this.$route.params.id);

    if(!this.model){
      this.$router.push('/admin/customers/list')
    }

    this.$store.commit("ui/changeHeader", {
      value: {
        threeDotMenu: [
          {
            text: this.$t("CreateCustomer"),
            fn: this.createCustomer.bind(this)
          },
          {
            text: this.$t("DeleteCustomer"),
            fn: this.deleteCustomer.bind(this)
          }
        ],
        text: { translate: false, value: this.model.consumerName },
        navBtn: { icon: 'mdi-chevron-left', fn: (() => this.$router.go(-1)).bind(this) }
      }
    });
  }
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