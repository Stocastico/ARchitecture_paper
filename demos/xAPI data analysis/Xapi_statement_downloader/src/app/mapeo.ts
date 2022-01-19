export class Mapeos {
  id: number
  orden: string;
  pin: string;
  value: string;

  constructor(id:number, orden: string, pin:string,value:string){
    this.id = id;
    this.orden = orden;
    this.pin = pin;
    this.value=value;
  }

}
