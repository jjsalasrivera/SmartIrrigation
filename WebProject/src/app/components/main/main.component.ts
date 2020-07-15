import { Component, OnInit } from '@angular/core';
import { MainService ,IrrigationRead } from 'src/app/services/main.service';
import {timer} from 'rxjs';

@Component({
  selector: 'app-main',
  templateUrl: './main.component.html',
  styleUrls: ['./main.component.css']
})

export class MainComponent implements OnInit {

  // public res: IrrigationRead = new IrrigationRead();

  public lastRead: Date;
  public lastIrrigation: Date;
  public status: string;
  public s1: number;
  public s2: number;
  public s3: number;
  public s4: number;
  public average: number;
  public minValue: number;
  public nextRead: Date;
  public minutes: number;
  public notes: string;

  constructor(private mainService: MainService) {
   }

  ngOnInit() {
    const source = timer(500, 5000);
    const subscribe = source.subscribe(() => { this.getLastValues(); });
  }

  getLastValues() {
    console.log('Get last Values');

    this.mainService.getLastValues().subscribe( (res: IrrigationRead) => {
      if ( res ) {
        this.lastRead = res.lastRead;
        this.s1 = res.s1;
        this.s2 = res.s2;
        this.s3 = res.s3;
        this.s4 = res.s4;
        this.average = res.average;
        this.minValue = res.minValue;
        this.nextRead = res.nextRead;
        this.lastIrrigation = res.lastIrrigation;
        this.status = res.status;
        this.minutes = res.minutesIrrigation;
        this.notes = res.notes;
      }
    });
  }

  forceRead() {
    console.log('Forced read');

    this.mainService.forceRead().subscribe( () => {
      console.log('Read forced');
    });
  }

  forceIrrigation() {
    console.log('Forced irrigation');

    this.mainService.forceIrrigation().subscribe( () => {
      console.log('Irrigation Forced');
    });
  }
}
