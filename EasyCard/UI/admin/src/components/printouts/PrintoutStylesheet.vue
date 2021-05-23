<template>
    <component v-bind:is="'style'">
       .printout{
        direction: rtl;
        font-size: 12px;
       }
       @media print {
        body {-webkit-print-color-adjust: exact;}
       }
      .print-entity{
        display: flex;
        padding: 0.15rem 0;
        border: 1px solid #c4c4c4;
        border-bottom: none;
      }
      .print-entity div{
        text-align: start;
        width: 50%;
        padding: 0 5px;
      }
      /*.print-entity:nth-child(2n){
        background: #f5f5f5 !important;
      }*/
      .print-entity:first-child{
        /*border-radius: 3px 3px 0 0;*/
      }
      .print-entity:last-child{
        border-bottom: 1px solid #c4c4c4;
        /*border-radius: 0 0 3px 3px;*/
      }
    </component>
</template>

<script>
    export default {
        
    }
</script>
