<template>
  <div class="frame">
    <canvas v-bind:id="canvasID" v-on:click="clicked" class="lsCanvas" height="75" width="100"></canvas>
  </div>
</template>

<script>
export default {
  name: 'ListCanvas',
  props: {
    msg: String,
    patternObj: Object,
    index: Number
  },
  data() {
    return {
      canvas: null,
      pattern: null,
      canvasID: null
    };
  },
  created(){
    this.pattern = this.patternObj.pattern
    this.canvasID = "canvas"+this.index;
  },

  mounted() {
    //give vue access to canvas after vue and DOM init
    let c = document.getElementById(this.canvasID);
    this.canvas = c.getContext('2d');
    this.canvas.strokeStyle = 'red';
    this.canvas.lineWidth = 2;
    if(this.pattern.length>0){
      //draw out pattern
      let startPoint = this.pattern[0]
      this.drawDot(startPoint.x/10,startPoint.y/10)
      var i;
      for (i = 1; i < this.pattern.length; i++) {
        let endPoint =this.pattern[i]
        this.drawLine(startPoint.x/10,startPoint.y/10,endPoint.x/10,endPoint.y/10);
        startPoint = endPoint;
      } 
    }
  },
  methods:{
    drawLine(startX,startY,endX,endY){
      this.canvas.beginPath();
      this.canvas.moveTo(startX, startY);
      this.canvas.lineTo(endX, endY);
      this.canvas.stroke();
    },
    drawDot(x,y){
      this.canvas.fillRect(x-1,y-1,2,2);
    },
    clicked(){
      this.$emit('clicked', this.index)
    }
  },
  computed:{}
}
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>
.lsCanvas {
    width: 100px;
    height: 75px;
    border: 1px dotted;
    background-image:url('../assets/gbgmapsm.png');
    background-repeat: no-repeat;
}
.frame{
  margin: 5px;
}
</style>
