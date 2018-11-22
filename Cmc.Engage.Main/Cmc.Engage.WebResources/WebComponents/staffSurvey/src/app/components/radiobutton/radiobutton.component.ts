import { Component, OnInit,Input } from '@angular/core';

@Component({
  selector: 'app-radiobutton',
  templateUrl: './radiobutton.component.html',
  styleUrls: ['./radiobutton.component.less']
})
export class RadiobuttonComponent implements OnInit {
  @Input() question: any;
  @Input() option: any;
  @Input() disabled: any;
  constructor() { }

  ngOnInit() {
    this.disabled=Promise.resolve(true);
  }

}
