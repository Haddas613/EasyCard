<template>
  <v-select :items="['en-US', 'he-IL']" v-model="$i18n.locale" @change="switchLanguage()"></v-select>
</template>

<script>
import localization from "../helpers/localization";

export default {
  name: "LangSwitcher",
  methods: {
    switchLanguage: function() {
      localization.switchLanguage(this.$i18n, this.$vuetify);
    }
  }
};
</script>

<style lang="scss" scoped>
</style>