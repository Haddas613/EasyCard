<template>
  <v-flex class="d-flex flex-column">
    <v-text-field
      v-model="model.dealReference"
      :counter="50"
      :rules="[vr.primitives.maxLength(50)]"
      :label="$t('DealReference')"
      @keydown.native.space.prevent
      outlined
      required
    ></v-text-field>
    <v-text-field
      v-model="model.consumerEmail"
      :label="$t('CustomerEmail')"
      :rules="[vr.primitives.required, vr.primitives.email]"
      outlined
      @keydown.native.space.prevent
    ></v-text-field>
    <v-text-field
      v-model="model.consumerPhone"
      :label="$t('CustomerPhone')"
      :rules="[vr.primitives.maxLength(50)]"
      outlined
    ></v-text-field>
    <v-text-field
      v-model="model.consumerAddress"
      :label="$t('CustomerAddress')"
      :rules="[vr.primitives.maxLength(50)]"
      outlined
    ></v-text-field>
    <v-textarea
      v-model="model.dealDescription"
      :counter="1024"
      outlined
      rows="3"
      :rules="[vr.primitives.required,  vr.primitives.maxLength(1024)]"
    >
      <template v-slot:label>
        <div>{{$t('DealDescription')}}</div>
      </template>
    </v-textarea>
  </v-flex>
</template>

<script>
import ValidationRules from "../../helpers/validation-rules";
import { mapState } from "vuex";

export default {
  components: { },
  props: {
    data: {
      type: Object,
      default: null,
      required: true
    }
  },
  data() {
    return {
      dictionaries: {},
      model: {
        ...this.data,
      },
      vr: ValidationRules,
      messageDialog: false
    };
  },
  async mounted() {
    // if(!this.model.dealDescription){
    //     this.model.dealDescription = this.terminalStore.settings.defaultChargeDescription;
    // }
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
  },
};
</script>