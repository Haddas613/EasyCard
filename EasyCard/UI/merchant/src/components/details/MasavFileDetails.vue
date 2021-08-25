<template>
  <v-card flat class="my-2">
    <v-card-title
      class="py-3 ecdgray--text subtitle-2 text-uppercase info-block-title"
    >{{$t("MasavFile")}}</v-card-title>
    <v-divider></v-divider>
    <v-card-text>
      <v-row class="info-container body-1 black--text">
        <v-col cols="12" :md="oneline ? '2' : '4'" class="info-block">
          <p class="caption ecgray--text text--darken-2">{{$t('ID')}}</p>
          <p>{{(model.masavFileID || '-')}}</p>
        </v-col>
        <v-col cols="12" :md="oneline ? '2' : '4'" class="info-block">
          <p class="caption ecgray--text text--darken-2">{{$t('PayedDate')}}</p>
          <p v-if="model.payedDate">{{model.payedDate | ecdate}}</p>
          <p v-else>-</p>
        </v-col>
        <v-col cols="12" :md="oneline ? '2' : '4'" class="info-block">
          <p class="caption ecgray--text text--darken-2">{{$t('Created')}}</p>
          <p v-if="model.masavFileDate">{{model.masavFileDate | ecdate('DD/MM/YYYY')}}</p>
          <p v-else>-</p>
        </v-col>
        <v-col cols="12" :md="oneline ? '2' : '4'" class="info-block">
          <p class="caption ecgray--text text--darken-2">{{$t('Total')}}</p>
          <p>{{model.totalAmount | currency(model.currency)}}</p>
        </v-col>
        <v-col cols="12" :md="oneline ? '2' : '4'" class="info-block">
          <p class="caption ecgray--text text--darken-2">{{$t('Terminal')}}</p>
          <p>{{model.terminalName || model.terminalID}}</p>
        </v-col>
        <v-col cols="12" :md="oneline ? '2' : '4'" class="info-block">
          <p class="caption ecgray--text text--darken-2">{{$t('Download')}}</p>
          <v-btn v-if="model.storageReference" outlined color="success" small :disabled="!model.storageReference" :title="$t('ClickToDownload')" @click="downloadMasavFile(model.storageReference)">
            <v-icon color="red" size="1.25rem">mdi-file-outline</v-icon>
          </v-btn>
          <span v-else>-</span>
        </v-col>
      </v-row>
    </v-card-text>
  </v-card>
</template>

<script>
export default {
  props: {
    model: {
      type: Object,
      required: true
    },
    consumerName: {
        type: String,
        required: false,
        default: null
    },
    oneline: {
      type: Boolean,
      default: false
    }
  },
  methods: {
      downloadMasavFile(link) {
          window.open(link, '_blank');
      }
  },
};
</script>