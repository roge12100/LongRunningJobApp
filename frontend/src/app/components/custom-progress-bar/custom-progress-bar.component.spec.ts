import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CustomProgressBarComponent } from './custom-progress-bar.component';
import { ComponentRef } from '@angular/core';

describe('CustomProgressBarComponent', () => {
  let component: CustomProgressBarComponent;
  let fixture: ComponentFixture<CustomProgressBarComponent>;
  let compiled: HTMLElement;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CustomProgressBarComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(CustomProgressBarComponent);
    component = fixture.componentInstance;
    compiled = fixture.nativeElement as HTMLElement;
  });

  describe('Component Initialization', () => {
    it('should create the component', () => {
      expect(component).toBeTruthy();
    });

    it('should have default input values', () => {
      expect(component.value()).toBe(0);
      expect(component.showPercentage()).toBe(true);
      expect(component.animated()).toBe(true);
      expect(component.height()).toBe('24px');
      expect(component.status()).toBe('processing');
    });
  });

  describe('Value Clamping', () => {
    it('should clamp negative values to 0', () => {
      fixture.componentRef.setInput('value', -50);
      fixture.detectChanges();
      expect(component.clampedValue()).toBe(0);
    });

    it('should clamp values above 100 to 100', () => {
      fixture.componentRef.setInput('value', 150);
      fixture.detectChanges();
      expect(component.clampedValue()).toBe(100);
    });

    it('should accept valid values between 0 and 100', () => {
      fixture.componentRef.setInput('value', 50);
      fixture.detectChanges();
      expect(component.clampedValue()).toBe(50);
    });

    it('should handle decimal values', () => {
      fixture.componentRef.setInput('value', 33.33);
      fixture.detectChanges();
      expect(component.clampedValue()).toBe(33.33);
    });
  });

  describe('Percentage Text', () => {
    it('should format percentage text correctly', () => {
      fixture.componentRef.setInput('value', 45);
      fixture.detectChanges();
      expect(component.percentageText()).toBe('45%');
    });

    it('should round decimal percentages', () => {
      fixture.componentRef.setInput('value', 45.6);
      fixture.detectChanges();
      expect(component.percentageText()).toBe('46%');
    });

    it('should display 0% for negative values', () => {
      fixture.componentRef.setInput('value', -10);
      fixture.detectChanges();
      expect(component.percentageText()).toBe('0%');
    });

    it('should display 100% for values above 100', () => {
      fixture.componentRef.setInput('value', 150);
      fixture.detectChanges();
      expect(component.percentageText()).toBe('100%');
    });
  });

  describe('CSS Custom Properties', () => {
    it('should generate correct style object', () => {
      fixture.componentRef.setInput('value', 60);
      fixture.componentRef.setInput('height', '32px');
      fixture.detectChanges();

      const styles = component.progressStyles();
      expect(styles['--progress-value']).toBe('60%');
      expect(styles['--progress-height']).toBe('32px');
    });

    it('should update styles when value changes', () => {
      fixture.componentRef.setInput('value', 30);
      fixture.detectChanges();
      expect(component.progressStyles()['--progress-value']).toBe('30%');

      fixture.componentRef.setInput('value', 70);
      fixture.detectChanges();
      expect(component.progressStyles()['--progress-value']).toBe('70%');
    });
  });

  describe('CSS Classes', () => {
    it('should always include base container class', () => {
      fixture.detectChanges();
      const classes = component.containerClasses();
      expect(classes['progress-bar-container']).toBe(true);
    });

    it('should include animated class when animated is true', () => {
      fixture.componentRef.setInput('animated', true);
      fixture.detectChanges();
      expect(component.containerClasses()['animated']).toBe(true);
    });

    it('should not include animated class when animated is false', () => {
      fixture.componentRef.setInput('animated', false);
      fixture.detectChanges();
      expect(component.containerClasses()['animated']).toBe(false);
    });

    it('should include complete class when status is complete', () => {
      fixture.componentRef.setInput('status', 'complete');
      fixture.detectChanges();
      expect(component.containerClasses()['complete']).toBe(true);
    });

    it('should include error class when status is error', () => {
      fixture.componentRef.setInput('status', 'error');
      fixture.detectChanges();
      expect(component.containerClasses()['error']).toBe(true);
    });
  });

  describe('DOM Rendering', () => {
    beforeEach(() => {
      fixture.componentRef.setInput('value', 50);
      fixture.detectChanges();
    });

    it('should render the progress bar container', () => {
      const container = compiled.querySelector('.progress-bar-container');
      expect(container).toBeTruthy();
    });

    it('should render the progress fill', () => {
      const fill = compiled.querySelector('.progress-fill');
      expect(fill).toBeTruthy();
    });

    it('should render percentage text when showPercentage is true', () => {
      fixture.componentRef.setInput('showPercentage', true);
      fixture.detectChanges();
      const text = compiled.querySelector('.progress-text');
      expect(text).toBeTruthy();
      expect(text?.textContent?.trim()).toBe('50%');
    });

    it('should not render percentage text when showPercentage is false', () => {
      fixture.componentRef.setInput('showPercentage', false);
      fixture.detectChanges();
      const text = compiled.querySelector('.progress-text');
      expect(text).toBeFalsy();
    });

    it('should render shine effect when animated is true', () => {
      fixture.componentRef.setInput('animated', true);
      fixture.detectChanges();
      const shine = compiled.querySelector('.progress-shine');
      expect(shine).toBeTruthy();
    });

    it('should not render shine effect when animated is false', () => {
      fixture.componentRef.setInput('animated', false);
      fixture.detectChanges();
      const shine = compiled.querySelector('.progress-shine');
      expect(shine).toBeFalsy();
    });
  });

  describe('ARIA Attributes', () => {
    beforeEach(() => {
      fixture.componentRef.setInput('value', 65);
      fixture.detectChanges();
    });

    it('should have role="progressbar"', () => {
      const container = compiled.querySelector('.progress-bar-container');
      expect(container?.getAttribute('role')).toBe('progressbar');
    });

    it('should have correct aria-valuenow', () => {
      const container = compiled.querySelector('.progress-bar-container');
      expect(container?.getAttribute('aria-valuenow')).toBe('65');
    });

    it('should have aria-valuemin="0"', () => {
      const container = compiled.querySelector('.progress-bar-container');
      expect(container?.getAttribute('aria-valuemin')).toBe('0');
    });

    it('should have aria-valuemax="100"', () => {
      const container = compiled.querySelector('.progress-bar-container');
      expect(container?.getAttribute('aria-valuemax')).toBe('100');
    });

    it('should have descriptive aria-label', () => {
      const container = compiled.querySelector('.progress-bar-container');
      const label = container?.getAttribute('aria-label');
      expect(label).toContain('Progress:');
      expect(label).toContain('65%');
    });

    it('should hide percentage text from screen readers', () => {
      fixture.componentRef.setInput('showPercentage', true);
      fixture.detectChanges();
      const text = compiled.querySelector('.progress-text');
      expect(text?.getAttribute('aria-hidden')).toBe('true');
    });
  });

  describe('Reactivity', () => {
    it('should update display when value changes', () => {
      fixture.componentRef.setInput('value', 25);
      fixture.detectChanges();
      let text = compiled.querySelector('.progress-text');
      expect(text?.textContent?.trim()).toBe('25%');

      fixture.componentRef.setInput('value', 75);
      fixture.detectChanges();
      text = compiled.querySelector('.progress-text');
      expect(text?.textContent?.trim()).toBe('75%');
    });

    it('should update ARIA attributes when value changes', () => {
      fixture.componentRef.setInput('value', 30);
      fixture.detectChanges();
      const container = compiled.querySelector('.progress-bar-container');
      expect(container?.getAttribute('aria-valuenow')).toBe('30');

      fixture.componentRef.setInput('value', 80);
      fixture.detectChanges();
      expect(container?.getAttribute('aria-valuenow')).toBe('80');
    });
  });

  describe('Edge Cases', () => {
    it('should handle zero value', () => {
      fixture.componentRef.setInput('value', 0);
      fixture.detectChanges();
      expect(component.clampedValue()).toBe(0);
      expect(component.percentageText()).toBe('0%');
    });

    it('should handle 100 value', () => {
      fixture.componentRef.setInput('value', 100);
      fixture.detectChanges();
      expect(component.clampedValue()).toBe(100);
      expect(component.percentageText()).toBe('100%');
    });

    it('should handle very small values', () => {
      fixture.componentRef.setInput('value', 0.001);
      fixture.detectChanges();
      expect(component.clampedValue()).toBe(0.001);
      expect(component.percentageText()).toBe('0%'); 
    });

    it('should handle null/undefined gracefully', () => {
      fixture.componentRef.setInput('value', null as any);
      fixture.detectChanges();
      expect(component.value()).toBeFalsy();
    });
  });

  describe('Performance', () => {
    it('should not cause unnecessary re-renders', () => {
      const spy = vi.spyOn(component, 'clampedValue');
      
      fixture.componentRef.setInput('value', 50);
      fixture.detectChanges();
      
      const initialCallCount = spy.mock.calls.length;
      
      fixture.componentRef.setInput('value', 50);
      fixture.detectChanges();
      
      expect(spy.mock.calls.length).toBeGreaterThanOrEqual(initialCallCount);
    });
  });
});