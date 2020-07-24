<template>
  <v-select :items="['en-US', 'he-IL']" v-model="currentLocale" outlined></v-select>
</template>

<script>
import { mapState } from 'vuex'

export default {
  name: "LangSwitcher",
  methods: {
  },
  computed: {
    ...mapState({
      currentLocaleStore: state => state.localization.currentLocale
    }),
    currentLocale: {
      get: function() { return this.currentLocaleStore },
      set: function(newValue){
        this.$store.commit('localization/changeLanguage', {vm: this, newLocale: newValue})
      }
    }
  },
};
</script>