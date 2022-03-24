<template>
  <div>
    <div v-if="model.webHooksConfiguration">
      <v-card>
        <v-card-title>{{ $t('SecurityHeader') }}</v-card-title>
        <v-card-text>
          <v-form>
            <v-row>
              <v-col cols="12" md="6" class="py-0">
                <v-text-field
                  v-model="model.webHooksConfiguration.securityHeader.key"
                  :label="$t('Key')"
                ></v-text-field>
              </v-col>
              <v-col cols="12" md="6" class="py-0">
                <v-text-field
                  v-model="model.webHooksConfiguration.securityHeader.value"
                  :label="$t('Value')"
                ></v-text-field>
              </v-col>
            </v-row>
          </v-form>
        </v-card-text>
        <v-card-title class="d-flex justify-space-between">
          <div>{{ $t('Webhooks') }}</div>
          <v-form class="pt-2" ref="form" v-model="valid">
            <div class="d-flex px-2 pt-4 justify-end">
              <v-select
                class="mx-2"
                :items="webhooks"
                item-text="name"
                item-value="value"
                v-model="selectedWebhookEventType"
                :label="$t('WebhookEvent')"
                outlined
              ></v-select>
              <v-btn
                color="primary"
                class="white--text"
                :block="$vuetify.breakpoint.smAndDown"
                @click="addWebhook()"
                :disabled="
                  !selectedWebhookEventType ||
                    !webhooks ||
                    !model.webHooksConfiguration ||
                    webhooks.length == model.webHooksConfiguration.webHooks.length
                "
                >{{ $t('AddWebhookEvent') }}</v-btn>
            </div>
          </v-form>
        </v-card-title>
        <v-card-text>
          <ec-list
            v-if="model.webHooksConfiguration.webHooks.length"
            :items="model.webHooksConfiguration.webHooks"
            dashed
            spaced
          >
            <template v-slot:left="{ item }">
              <v-col cols="12" md="6" class="px-2 text-start text-oneline">
                <span class="body-1">{{ item.eventName }}</span>
              </v-col>
              <v-col cols="12" md="6" class="text-align-initial font-weight-bold subtitle-2">
                {{ $t('FailureEvent') }} :
                <span
                  :class="{
                    'error--text': item.isFailureEvent,
                    'success--text': !item.isFailureEvent,
                  }"
                >
                  {{ item.isFailureEvent ? $t('Yes') : $t('No') }}
                </span>
              </v-col>
            </template>
            <template v-slot:right="{ item }">
              <v-col cols="12" md="12" class="text-start text-oneline">
                <span class="body-1"
                  ><v-text-field v-model="item.url" :label="$t('URL')"></v-text-field
                ></span>
              </v-col>
            </template>

            <template v-slot:append="{ item }">
              <v-col cols="12" class="pt-2 px-4 text-end font-weight-bold subtitle-2">
                <v-btn
                  class="mx-1"
                  color="error"
                  outlined
                  small
                  @click="deleteWebhookEvent(item.entityType)"
                >
                  <v-icon small>mdi-delete</v-icon>
                </v-btn>
              </v-col>
            </template>
          </ec-list>
          <p v-else>{{ $t('NothingToShow') }}</p>
        </v-card-text>
        <v-card-actions class="d-flex justify-end">
          <v-btn color="success" @click="save()">{{$t('Save')}}</v-btn>
        </v-card-actions>
      </v-card>
    </div>
  </div>
</template>

<script>
import ValidationRules from '../../helpers/validation-rules';
export default {
  components: {
    EcList: () => import("@/components/ec/EcList.vue"),
  },
  props: {
    terminal: {
      type: Object,
      required: true,
    },
  },
  data() {
    return {
      model: { ...this.terminal },
      webhooks: [],
      selectedWebhookEventType: null,
      webhookDialog: false,
      valid: false,
      vr: ValidationRules,
    };
  },
  async mounted() {
    if (!this.model.webHooksConfiguration) {
      this.$set(this.model, 'webHooksConfiguration', {
        securityHeader: {
          key: null,
          value: null,
        },
        webHooks: [],
      });
    }
    let webhooks = await this.$api.dictionaries.getWebhooks();
    this.webhooks = webhooks.map((e) => {
      return {
        name: this.$t(e.eventName),
        value: e.entityType,
        ...e,
        disabled: this.lodash.some(
          this.model.webHooksConfiguration.webHooks,
          (t) => t.entityType == e.entityType
        ),
      };
    });
  },
  methods: {
    async addWebhook() {
      if (!this.selectedWebhookEventType) {
        return;
      }
      let wh = this.lodash.find(
        this.webhooks,
        (t) => t.entityType == this.selectedWebhookEventType
      );
      wh.disabled = true;

      this.model.webHooksConfiguration.webHooks.push(this.mapWebhook(wh));
    },
    mapWebhook(wh) {
      return {
        eventName: wh.eventName,
        entityType: wh.entityType,
        isFailureEvent: wh.isFailureEvent,
        url: wh.url,
      };
    },
    async deleteWebhookEvent(entityType) {
      let idx = this.lodash.findIndex(
        this.model.webHooksConfiguration.webHooks,
        (t) => t.entityType == entityType
      );

      let wh = this.lodash.find(this.webhooks, (t) => t.entityType == entityType);
      wh.disabled = false;
      this.model.webHooksConfiguration.webHooks.splice(idx, 1);
    },
    async save(){
      this.$emit('save', this.model.webHooksConfiguration);
    },
  },
};
</script>
