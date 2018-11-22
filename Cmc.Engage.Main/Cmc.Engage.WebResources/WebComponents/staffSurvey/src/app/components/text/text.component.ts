import { Component, OnInit ,Input} from '@angular/core';

@Component({
  selector: 'app-text',
  templateUrl: './text.component.html',
  styleUrls: ['./text.component.less']
})
export class TextComponent implements OnInit {
  @Input() question: any;
  @Input() answer: any;
  @Input() disabled: any;
  constructor() { }

  ngOnInit() {
    this.disabled=Promise.resolve(true);
  }

}
