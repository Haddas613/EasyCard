<template>
  <v-select :items="['en-IL', 'he-IL']" v-model="currentLocale" outlined></v-select>
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
      set: async function(newValue){
        await this.$store.commit('localization/changeLanguage', {vm: this, newLocale: newValue});
        
        // retrieve all dictionaries with new locale
        location.reload();
      }
    }
  },
};
</script>