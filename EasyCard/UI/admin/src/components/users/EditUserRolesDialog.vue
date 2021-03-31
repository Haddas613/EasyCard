<template>
  <ec-dialog :dialog.sync="visible">
    <template v-slot:title>{{$t('UserRoles')}}</template>
    <template>
      <v-form class="pt-2" ref="form" v-model="valid" lazy-validation>
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
      model: { ...this.user },
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
      // this.loading = true;
      let payload = {
        userID: this.model.$userID || this.model.userID,
        roles: this.$refs.userRolesRef.getData().roles
      }

      let operationResult = await this.$api.users.updateUserRoles(payload);
      if (operationResult.status === "success") {
        this.$emit("ok");
      }
      this.visible = false;
      this.loading = false;
    }
  }
};
</script>