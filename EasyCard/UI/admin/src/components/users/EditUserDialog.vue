<template>
  <ec-dialog :dialog.sync="visible">
    <template v-slot:title>{{$t('EditUser')}}</template>
    <template>
      <v-form class="pt-2" ref="form" v-model="valid" lazy-validation v-if="model">
        <v-row class="pb-4">
          <v-col cols="12" md="4" class="py-0">
            <v-text-field
              v-model="model.firstName"
              :counter="50"
              :rules="[vr.primitives.required, vr.primitives.stringLength(3, 50)]"
              :label="$t('FirstName')"
            ></v-text-field>
          </v-col>
          <v-col cols="12" md="4" class="py-0">
            <v-text-field
              v-model="model.lastName"
              :counter="50"
              :rules="[vr.primitives.required, vr.primitives.stringLength(3, 50)]"
              :label="$t('LastName')"
            ></v-text-field>
          </v-col>
          <v-col cols="12" md="4" class="py-0">
            <v-text-field
              v-model="model.phoneNumber"
              :counter="50"
              :rules="[vr.primitives.stringLength(3, 50)]"
              :label="$t('Phone')"
              type="number"
            ></v-text-field>
          </v-col>
        </v-row>
        <v-row>
          <user-roles-fields :user="user" ref="userRolesRef"></user-roles-fields>
        </v-row>
      </v-form>
      <div class="d-flex px-2 pt-4 justify-end">
        <v-btn
          color="primary"
          class="white--text"
          :block="$vuetify.breakpoint.smAndDown"
          :loading="loading"
          @click="ok()"
        >{{$t("OK")}}</v-btn>
      </div>
    </template>
  </ec-dialog>
</template>

<script>
import ValidationRules from "../../helpers/validation-rules";
export default {
  props: {
    user: {
      type: Object,
      default: null,
      required: true
    },
    show: {
      type: Boolean,
      default: false,
      required: true
    }
  },
  components: {
    EcDialog: () => import("../../components/ec/EcDialog"),
    UserRolesFields: () => import("./UserRolesFIelds")
  },
  data() {
    return {
      model: null,
      loading: false,
      valid: true,
      vr: ValidationRules,
    };
  },
  computed: {
    visible: {
      get: function() {
        return this.show;
      },
      set: function(val) {
        this.$emit("update:show", val);
      }
    }
  },
  async mounted () {
    this.getUser();
  },
  watch: {
    async user(newValue, oldValue) {
      this.getUser();
    }
  },
  methods: {
    async getUser(){
      this.loading = true;
      this.model = await this.$api.users.getUser(this.user.$userID || this.user.userID);
      this.loading = false;
    },
    async ok() {
      if (!this.$refs.form.validate()) {
        return;
      }
      // this.loading = true;
      let payload = {
        phoneNumber: this.model.phoneNumber,
        firstName: this.model.firstName,
        lastName: this.model.lastName,
        userID: this.model.$userID || this.model.userID,
        roles: this.$refs.userRolesRef.getData().roles
      }

      let operationResult = await this.$api.users.updateUser(payload);
      if (operationResult.status === "success") {
        this.$emit("ok");
      }
      this.visible = false;
      this.loading = false;
    }
  }
};
</script>