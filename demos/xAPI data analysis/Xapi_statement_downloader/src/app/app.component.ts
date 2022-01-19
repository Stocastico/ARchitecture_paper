import { Component } from '@angular/core';
import { Mapeos } from './mapeo';
import { HttpClient,HttpHeaders } from '@angular/common/http';
import {MatDatepickerModule, MatNativeDateModule } from '@angular/material';
import { DatePipe } from '@angular/common';
const httpOptions = {
  headers: new HttpHeaders({ 
    'Content-Type':'application/json',
    'Access-Control-Allow-Origin':'*',
    'Access-Control-Allow-Methods':'GET, POST, OPTIONS, PUT, PATCH, DELETE'
  })
};
@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'mapeo-pins';
  command: string;
  selectedPin: string;
  selectedValue: string;
  mapa1 :Mapeos;
  //public commands:string[]=[];
  commands: Mapeos[] = [];
  cmd:string;
  id:number;
  sendJSONvar: string="";
  constructor(public http: HttpClient) { }
  NewCommand(){
    
    //this.commands.forEach
    //console.log(this.commands[0]);
  }
  delete(id:number){
    console.log(id);
    this.commands.splice(id-1,1);
    console.log(this.commands);
    this.commands.forEach(command => {
      if(command.id > id){
        command.id = command.id-1;
      }
    });
  }
  sendJSON(){
    this.id = this.commands.length+1;
    console.log(this.cmd);
    console.log(this.selectedPin);
    console.log(this.selectedValue);
    this.selectedPin=new DatePipe('en').transform(this.selectedPin, 'yyyy-MM-dd') + "T" +new DatePipe('en').transform(this.selectedPin,'hh:mm:ss') + "Z"; 
    //this.selectedPin=new DatePipe('en').transform(this.selectedPin, 'yyyy-MM-dd') + "T" +"00:00:00" + "Z"; 
    console.log(this.selectedPin);
    this.selectedValue=new DatePipe('en').transform(this.selectedValue, 'yyyy-MM-dd') + "T" +new DatePipe('en').transform(this.selectedValue,'hh:mm:ss') + "Z"; 
    //this.selectedValue=new DatePipe('en').transform(this.selectedValue, 'yyyy-MM-dd') + "T" +"00:00:00" + "Z"; 
    console.log(this.selectedValue);
    this.mapa1 = new Mapeos(this.id, this.cmd, this.selectedPin, this.selectedValue);
    this.commands.push(this.mapa1);
    this.http.post<any>('http://127.0.0.1:5000/new_commands', this.commands).subscribe(data=>{console.log("send")})
  }
  obtainJSON(){
    //window.location.href = 'http://localhost:8082/assets/statements.json'
    this.http.post<any>('http://127.0.0.1:5000/read_json',this.sendJSONvar).subscribe(data => {console.log(data); 
      this.sendJSONvar=JSON.stringify(data);var uri = "data:application/json;charset=UTF-8," + encodeURIComponent(this.sendJSONvar);
      var a = document.createElement('a');
      a.href = uri;
      a.innerHTML = "Download statements.json";
      a.download="statements.json"
      a.click();
      document.body.appendChild(a);
      document.body.style.textAlign = "center";});
    console.log(this.sendJSONvar)
  }
}
