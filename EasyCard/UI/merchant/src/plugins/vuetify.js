import Vue from 'vue';
import Vuetify from 'vuetify';
import 'vuetify/dist/vuetify.min.css';
import '@mdi/font/css/materialdesignicons.css'
import colors from 'vuetify/lib/util/colors'

import he from 'vuetify/es5/locale/he';
import en from 'vuetify/es5/locale/en';

Vue.use(Vuetify);

const VBtn = Vue.component('VBtn')
VBtn.options.props.ripple.default = false

export default new Vuetify({
  theme: {
      options: {
        customProperties: true,
      },
    themes: {
      light: {
        primary: '#0f99c7', //light blue
        secondary: '#0f66ad', //dark blue
        accent: '#ffc700', //yellow
        error: '#FF5252',
        info: '#00b5f1',
        success: '#4CAF50',
        warning: '#FFC107',

        //easy card custom colors
        ecbg: '#f4f6f7',
        ecgray: colors.grey.lighten1,
        ecdgray: colors.grey.darken2, //dark gray
        eclgray: '#e0e0e0', //light gray
        ecnavLink: colors.grey.darken4,
        ecred: colors.red.darken2
      },
      dark:{
        primary: colors.blue.darken4,
        secondary: '#0f66ad', //dark blue
        accent: '#ffc700', //yellow
        error: '#FF5252',
        info: '#00b5f1',
        success: '#4CAF50',
        warning: '#FFC107',

        //easy card custom colors
        ecbg: colors.shades.black,
        ecnavLink: colors.grey.lighten4,
        ecdgray: colors.grey.darken2, //dark gray
        eclgray: '#e0e0e0', //light gray
        ecred: colors.red.darken2
      }
    },
  },
  icons: {
    iconfont: 'mdi',
  },
  lang:{
    locales: {en, he}
  }
});
