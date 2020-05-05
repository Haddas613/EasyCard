import Vue from 'vue';
import Vuetify from 'vuetify';
import 'vuetify/dist/vuetify.min.css';
import '@mdi/font/css/materialdesignicons.css'

Vue.use(Vuetify);

export default new Vuetify({
  theme: {
      options: {
        customProperties: true,
      },
    themes: {
      light: {
        primary: '#059ada', //light blue
        secondary: '#0f66ad', //dark blue
        accent: '#ffc700', //yellow
        error: '#FF5252',
        info: '#00b5f1',
        success: '#4CAF50',
        warning: '#FFC107',
        
        //easy card custom colors
        ecbg: '#888888'
      },
    },
  },
  icons: {
    iconfont: 'mdi',
  },
});
