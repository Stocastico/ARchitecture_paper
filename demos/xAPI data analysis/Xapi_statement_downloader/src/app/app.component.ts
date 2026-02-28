import { Component } from '@angular/core';
import { Mapeos } from './mapeo';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { DatePipe } from '@angular/common';

const httpOptions = {
  headers: new HttpHeaders({
    'Content-Type': 'application/json',
    'Access-Control-Allow-Origin': '*',
    'Access-Control-Allow-Methods': 'GET, POST, OPTIONS, PUT, PATCH, DELETE'
  })
};

@Component({
  selector: 'app-root',
  standalone: false,
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'mapeo-pins';
  command = '';
  selectedPin = '';
  selectedValue = '';
  mapa1!: Mapeos;
  commands: Mapeos[] = [];
  cmd = '';
  id = 0;
  sendJSONvar = '';

  constructor(public http: HttpClient) { }

  NewCommand(): void {
    // placeholder for future command logic
  }

  delete(id: number): void {
    console.log(id);
    this.commands.splice(id - 1, 1);
    console.log(this.commands);
    this.commands.forEach(command => {
      if (command.id > id) {
        command.id = command.id - 1;
      }
    });
  }

  sendJSON(): void {
    this.id = this.commands.length + 1;
    console.log(this.cmd);
    console.log(this.selectedPin);
    console.log(this.selectedValue);
    this.selectedPin = new DatePipe('en').transform(this.selectedPin, 'yyyy-MM-dd') + 'T' + new DatePipe('en').transform(this.selectedPin, 'hh:mm:ss') + 'Z';
    console.log(this.selectedPin);
    this.selectedValue = new DatePipe('en').transform(this.selectedValue, 'yyyy-MM-dd') + 'T' + new DatePipe('en').transform(this.selectedValue, 'hh:mm:ss') + 'Z';
    console.log(this.selectedValue);
    this.mapa1 = new Mapeos(this.id, this.cmd, this.selectedPin, this.selectedValue);
    this.commands.push(this.mapa1);
    this.http.post<unknown>('http://127.0.0.1:5000/new_commands', this.commands).subscribe(() => { console.log('send'); });
  }

  obtainJSON(): void {
    this.http.post<unknown>('http://127.0.0.1:5000/read_json', this.sendJSONvar).subscribe(data => {
      console.log(data);
      this.sendJSONvar = JSON.stringify(data);
      const uri = 'data:application/json;charset=UTF-8,' + encodeURIComponent(this.sendJSONvar);
      const a = document.createElement('a');
      a.href = uri;
      a.innerHTML = 'Download statements.json';
      a.download = 'statements.json';
      a.click();
      document.body.appendChild(a);
      document.body.style.textAlign = 'center';
    });
    console.log(this.sendJSONvar);
  }
}
