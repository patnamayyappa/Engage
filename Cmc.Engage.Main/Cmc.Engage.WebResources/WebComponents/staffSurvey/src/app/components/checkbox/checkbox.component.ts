import { Component, OnInit,Input } from '@angular/core';

@Component({
  selector: 'app-checkbox',
  templateUrl: './checkbox.component.html',
  styleUrls: ['./checkbox.component.less']
})
export class CheckboxComponent implements OnInit {
  @Input() disabled: any;
  @Input() question: any;
  @Input() option: any;
  constructor() { }

  ngOnInit() {
    this.disabled=Promise.resolve(true);
  }

}
