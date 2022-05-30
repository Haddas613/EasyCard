<template>
  <v-flex class="d-flex flex-column">
    <v-row no-gutters>
      <v-col cols="12" md="6">
        <v-text-field
          v-model="model.consumerName"
          :counter="50"
          :rules="[vr.primitives.requiredDepends(fieldsRequired), vr.primitives.maxLength(50)]"
          :label="$t('CustomerName')"
          outlined
        ></v-text-field>
      </v-col>
      <v-col cols="12" md="6">
        <v-text-field
          v-model="model.consumerEmail"
          :label="$t('CustomerEmail')"
          :rules="[vr.primitives.requiredDepends(fieldsRequired), vr.primitives.email]"
          outlined
          @keydown.native.space.prevent
          v-bind:class="{'px-1' : $vuetify.breakpoint.mdAndUp}"
        ></v-text-field>
      </v-col>
      <v-col cols="12" md="6">
        <v-text-field
          v-model="model.consumerPhone"
          :label="$t('CustomerPhone')"
          :rules="[vr.primitives.maxLength(50)]"
          outlined
        ></v-text-field>
      </v-col>
      <v-col cols="12" md="6">
        <v-text-field
          v-if="model.consumerAddress"
          v-model="model.consumerAddress.street"
          :label="$t('CustomerAddress')"
          :rules="[vr.primitives.maxLength(50)]"
          v-bind:class="{'px-1' : $vuetify.breakpoint.mdAndUp}"
          outlined
        ></v-text-field>
      </v-col>
      <v-col cols="12">
        <v-textarea
          v-model="model.dealDescription"
          :counter="250"
          outlined
          rows="3"
          :rules="[vr.primitives.maxLength(1024)]"
        >
          <template v-slot:label>
            <div>{{$t('DealDescription')}}</div>
          </template>
        </v-textarea>
      </v-col>
    </v-row>
  </v-flex>
</template>

<script>
import ValidationRules from "../../helpers/validation-rules";
import { mapState } from "vuex";

export default {
  components: {},
  props: {
    data: {
      type: Object,
      default: null,
      required: true,
    },
    isPaymentRequest: {
      type: Boolean,
      required: false,
      default: false
    },
    required: {
      type: Boolean,
      default: false,
    },
  },
  data() {
    return {
      dictionaries: {},
      model: {
        consumerAddress:{
          city: null,
          zip: null,
          street: null
        },
        ...this.data.dealDetails
      },
      saveCreditCard: this.data.saveCreditCard,
      vr: ValidationRules,
      messageDialog: false
    };
  },
  async mounted() {
    // if(!this.model.dealDescription){
    //     this.model.dealDescription = this.terminalStore.settings.defaultChargeDescription;
    // }
    if(!this.model.consumerAddress){
      this.model.consumerAddress = {
        city: null,
        zip: null,
        street: null
      };
    }
  },
  methods: {
    getData() {
      let result = { ...this.model };
      return result;
    }
  },
  computed: {
    // ...mapState({
    //   terminalStore: state => state.settings.terminalStore
    // })
    fieldsRequired() {
      return this.required || this.saveCreditCard;
    },
  }
};
</script>