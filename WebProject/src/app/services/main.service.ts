import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

export class IrrigationRead {
  lastRead: Date;
  s1: number;
  s2: number;
  s3: number;
  s4: number;
  average: number;
  minValue: number;
  nextRead: Date;
  lastIrrigation: Date;
  status: string;
  minutesIrrigation: number;
  notes: string;
}

@Injectable({
  providedIn: 'root'
})
export class MainService {

  public url = '';
  constructor(public http: HttpClient) { }

  getLastValues(): Observable<IrrigationRead> {
    return this.http.get<IrrigationRead>(this.url + '/api/last-values');
  }

  forceRead(): Observable<any> {
    return this.http.post<any>(this.url + '/api/force-read', null);
  }

  /*
  forceRead(): Observable<IrrigationRead> {
    return this.http.get<IrrigationRead>(this.url + '/api/force-read');
  }
  */

  forceIrrigation(): Observable<any> {
    return this.http.post<any>(this.url + '/api/force-irrigation', null );
  }

  /*
  forceIrrigation(value: number): Observable<any> {
    const headers = new HttpHeaders().set('Content-Type', 'application/json');
    const params = {value};
    return this.http.post<any>(this.url + '/api/force-irrigation/', params, {headers} );
  }
  */
}
