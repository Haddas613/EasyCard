<template>
  <span>
    <template v-if="schedule && deconstructed">
      <span>
        <v-chip
          dark
          color="primary"
          class="caption mx-1 mt-1"
        >{{$t("Repeat")}}/{{$t(obj.repeat.text)}} : {{obj.repeat.val}}</v-chip>
        
        <v-chip dark color="secondary" class="caption mx-1 mt-1" v-if="obj.start">
          {{$t("Start")}}/{{$t(obj.start.text)}}
          <span v-if="obj.start.text != 'Today'">: {{obj.start.val | ecdate("L")}}</span>
        </v-chip>

        <v-chip dark color="teal darken-3" class="caption mx-1 mt-1" v-if="obj.end">
          {{$t("End")}}/{{$t(obj.end.text)}}
          <span
            v-if="obj.end.text != 'Never'"
          >: {{obj.end.val | ecdate("L")}}</span>
        </v-chip>

        <v-chip
          dark
          color="teal darken-4"
          class="caption mx-1 mt-1"
          v-if="obj.numOfPayments"
        >{{$t(obj.numOfPayments.text)}} : {{obj.numOfPayments.val}}</v-chip>
      </span>
    </template>
    <template v-else>
      <span>{{$t(replacementText)}}</span>
    </template>
  </span>
</template>

<script>
export default {
  props: {
    schedule: {
      type: Object,
      default: () => null,
      required: true
    },
    replacementText: {
      type: String,
      default: "ScheduleIsNotDefined",
      required: false
    }
  },
  data() {
    return {
      obj: null
    };
  },
  computed: {
    deconstructed() {
      if (this.schedule == null || !this.schedule.repeatPeriodType) {
        return (this.obj = null);
      }
      const s = this.schedule;
      let res = {
        repeat: { text: s.repeatPeriodType, val: s.repeatPeriod }
      };
      if (s.startAtType) {
        res.start = { text: s.startAtType, val: s.startAt };
      }
      if (s.endAtType) {
        res.end = { text: s.endAtType, val: s.endAt };
      }
      if (s.endAtNumberOfPayments) {
        res.numOfPayments = {
          text: "NumberOfPayments",
          val: s.endAtNumberOfPayments
        };
      }
      return (this.obj = res);
    }
  }
};
</script>