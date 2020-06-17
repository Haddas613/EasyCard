<template>
  <v-card class="ec-card d-flex flex-column">
    <v-card-text class="py-2">
      <v-form class="ec-form" ref="form">
          <v-text-field
            v-model="model.name"
            :label="$t('Name on card')"
            :rules="[vr.primitives.required, vr.primitives.stringLength(3, 50)]"
            background-color="white"
            type="text"
            outlined
          >
           <template v-slot:append>
               <v-icon class="accent--text">mdi-camera</v-icon>
           </template>
          </v-text-field>

          <v-text-field
            v-model="model.cardNumber"
            :label="$t('CardNumber')"
            :rules="[vr.primitives.required, vr.primitives.stringLength(10, 19)]"
            background-color="white"
            placeholder="XXXX XXXX XXXX XXXX"
            type="text"
            outlined
            @keydown.native.space.prevent
          ></v-text-field>
          <v-text-field
            v-model="model.expiry"
            :label="$t('Expiry')"
            :rules="[vr.primitives.required]"
            background-color="white"
            type="text"
            placeholder="MM/YY"
            outlined
            @keydown.native.space.prevent
          ></v-text-field>
          <v-text-field
            v-model="model.cvv"
            :label="$t('CVV')"
            background-color="white"
            type="text"
            placeholder="XXX"
            outlined
            :rules="vr.complex.cvv"
            required
            @keydown.native.space.prevent
          ></v-text-field>

          <v-checkbox class="py-0 my-0" v-model="model.save" :label="$t('SaveCard')"></v-checkbox>
      </v-form>
    </v-card-text>
    <v-card-actions>
        <v-btn color="primary" class="px-2" bottom :x-large="true" fixed block @click="ok()">{{$t('Refund')}}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script>
import ValidationRules from "../../helpers/validation-rules";

export default {
    data() {
        return {
            model: {},
            vr: ValidationRules
        }
    },
    methods: {
        ok() {
            if (!this.$refs.form.validate()) return;
            this.$emit('ok', this.model);
        }
    },
};
</script>

<style lang="scss" scoped>

.ec-form{
    .v-input{
        padding-top: 0.5em;
    }
}
</style>