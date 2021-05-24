<template>
  <ec-dialog :dialog.sync="visible">
    <template v-slot:title>{{$t('CreateUser')}}</template>
    <template>
      <v-form class="pt-2" ref="form" v-model="valid" lazy-validation>
        <v-row>
          <v-col cols="12" class="py-0">
            <v-text-field
              v-model="model.email"
              :counter="50"
              :rules="[vr.primitives.required, vr.primitives.email]"
              :label="$t('Email')"
            ></v-text-field>
          </v-col>
          <v-col cols="12" class="py-0">
            <v-textarea
              v-model="model.inviteMessage"
              :counter="512"
              :rules="[vr.primitives.maxLength(512)]"
              :label="$t('InviteMessage')"
            ></v-textarea>
          </v-col>
          <v-col cols="12" class="py-0">
            <p class="subtitle-1">{{$t("Roles")}}</p>
            <user-roles-fields :user="model" ref="userRolesRef"></user-roles-fields>
          </v-col>
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
import appConstants from "../../helpers/app-constants";

export default {
  props: {
    merchantId: {
      type: String,
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
      model: {
        email: null,
        merchantID: this.merchantId,
        inviteMessage: null,
        roles: [appConstants.users.roles.merchant]
      },
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
  methods: {
    async ok() {
      if (!this.$refs.form.validate()) {
        return;
      }

      this.model.roles = this.$refs.userRolesRef.getData().roles;

      this.loading = true;
      let operationResult = await this.$api.users.inviteUser(this.model);
      if (operationResult.status === "success") {
        this.model.email = null;
        this.model.inviteMessage = null;
        this.model.roles = [appConstants.users.roles.merchant];
      }else {
        this.$toasted.show(operationResult.message, { type: "error" });
      }
      this.$emit("ok");
      this.visible = false;
      this.loading = false;
    }
  }
};
</script>