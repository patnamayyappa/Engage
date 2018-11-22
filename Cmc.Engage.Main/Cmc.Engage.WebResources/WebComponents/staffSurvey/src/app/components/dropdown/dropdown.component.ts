import { Component, OnInit ,Input} from '@angular/core';

@Component({
  selector: 'app-dropdown',
  templateUrl: './dropdown.component.html',
  styleUrls: ['./dropdown.component.less']
})
export class DropdownComponent implements OnInit {
  @Input() question: any;
  @Input() disabled: any;
  constructor() { }

  ngOnInit() {
    this.disabled=Promise.resolve(true);
  }

}
