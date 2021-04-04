<template>
  <div>
    <div class="frame">
      <canvas id="mapCanvas" @mouseup="setStartPoint" height="750" width="1000"></canvas>
    </div>
    <b-container fluid class="interface">
      <b-row>
        <b-col class="textfield">
            <input v-model="degrees" @input="onlyNumbers" placeholder="DEGREES">
        </b-col>
        <b-col class="buttons">
            <button v-on:click="move" :disabled="!validInput">MOVE</button>
            <button v-on:click="save" :disabled="!saveable">SAVE</button>
        </b-col>
      </b-row>
      <b-row class="patterns">
            <ListCanvas
                v-for="(pattern, index) in patterns"
                :key="pattern.id"
                :index="index"
                :patternObj="pattern"
                @clicked="onClickListItem"
            />
      </b-row>
    </b-container>
  </div>
</template>

<script>
import ListCanvas from './ListCanvas.vue'

export default {
  name: 'Interface',
  components: {
    ListCanvas
  },
  props: {
    msg: String,
  },
  data() {
    return {
      canvas: null,
      degrees: null,
      startingPoint: [{x:518, y:423}],
      currentPattern: [{x:518, y:423}],
      patterns: null
    };
  },
async created() {
    // GET stored patterns from backend using fetch and async req
    const response = await fetch("/patterns");
    const data = await response.json();
    console.log(data)
    this.patterns = data;

  },
  mounted() {
    //give vue access to canvas after vue and DOM init
    let c = document.getElementById("mapCanvas");
    this.canvas = c.getContext('2d');
    this.canvas.strokeStyle = 'red';
    this.canvas.lineWidth = 3;
    this.drawDot(this.startingPoint[0].x,this.startingPoint[0].y);
  },
  methods:{
    move(){
      //calculate endpoint for 20px line in direction of DEGREES and draw to canvas
      let lp = this.calcLinePoints();
      this.drawLine(lp.startX,lp.startY,lp.endX,lp.endY);
      this.currentPattern.push({x: lp.endX, y: lp.endY})
    },
    async save(){      
      //POST just saved pattern to backend using fetch and async/await req
      const requestOptions = {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({pattern: this.currentPattern})
      };
      fetch("/pattern", requestOptions)
        .then(response => response.json())
        .then(data => (this.patterns = data));

      //push to local patterns, reset and redraw canvas
      this.currentPattern = this.startingPoint;
      this.drawDot(this.startingPoint.x,this.startingPoint.x);
      
    },
    onlyNumbers: function() {
      // enforce that input only consists of numbers, maximum 3 digits and that its in the range 0-360
       this.degrees = this.degrees.replace(/[^0-9]/g,'');
       if(this.degrees.length>3){
         this.degrees = this.degrees.slice(0,3);
       }
       if(this.degrees>360){
         this.degrees = this.degrees % 360;
       }
    },
    setStartPoint(event){
      //allow user to set startpoint for first line
      if(this.currentPattern.length<2){
          this.currentPattern[0] = {x: event.offsetX, y: event.offsetY}
          this.drawDot(event.offsetX, event.offsetY)
      }
    },
    calcLinePoints(){
      //calculate endpoints using trig standard trig formulas and user input degrees as radians
      const prevEndPoint = this.currentPattern[this.currentPattern.length-1];
      let startX = prevEndPoint.x;
      let startY = prevEndPoint.y;
      let endX   = startX + 20 * Math.sin(this.radDegrees);
      let endY   = startY - 20 * Math.cos(this.radDegrees);
      return {startX,startY,endX,endY};
    },
    drawLine(startX,startY,endX,endY){
      this.canvas.beginPath();
      this.canvas.moveTo(startX, startY);
      this.canvas.lineTo(endX, endY);
      this.canvas.stroke();
    },
    drawDot(x,y){
      this.canvas.clearRect(0, 0, 1000, 750);
      this.canvas.fillRect(x-4,y-4,8,8);
    },
    onClickListItem (index) {
      //sets current pattern to a saved one, triggered by clicking list child component
      this.currentPattern = this.patterns[index].pattern;
      this.drawDot(this.currentPattern[0].x, this.currentPattern[0].y);
      
      let startPoint = this.currentPattern[0]
      var i;
      for (i = 1; i < this.currentPattern.length; i++) {
        let endPoint =this.currentPattern[i]
        this.drawLine(startPoint.x,startPoint.y,endPoint.x,endPoint.y);
        startPoint = endPoint;
      } 
    }
    },
    computed:{
    numDegrees: function () {
        return parseInt(this.degrees);
    },
    radDegrees: function (){
      return this.numDegrees * Math.PI / 180 
    },
    validInput: function (){
      return this.numDegrees<=360;
    },
    saveable: function(){
      return this.currentPattern.length>1
    }
  }
}
</script>

<style scoped>
#mapCanvas {
    width: 1000px;
    height: 750px;
    border: 1px dotted;
    background-image:url('../assets/gbgmap.png');
    background-repeat: no-repeat;
}
.interface{
  width: 1000px;
  text-align: center;
}
.patterns{
  border: 1px dotted;
  margin-bottom: 20px;
  min-height: 85px;
}
.row{
margin-top: 20px;
}
</style>
