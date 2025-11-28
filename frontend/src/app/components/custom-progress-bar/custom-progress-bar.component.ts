import { Component, input, computed, effect } from '@angular/core';
import { CommonModule } from '@angular/common';

/**
 * Custom Progress Bar Component
 * 
 * A production-ready, accessible progress bar built with vanilla HTML/CSS.
 * Uses Angular Signals for optimal performance and reactivity.
 * 
 * @example
 * <app-custom-progress-bar
 *   [value]="progress()"
 *   [showPercentage]="true"
 *   [animated]="true">
 * </app-custom-progress-bar>
 */
@Component({
  selector: 'app-custom-progress-bar',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './custom-progress-bar.component.html',
  styleUrls: ['./custom-progress-bar.component.css']
})
export class CustomProgressBarComponent {
  /**
   * Progress value (0-100)
   */
  value = input<number>(0);

  /**
   * Show percentage text overlay
   */
  showPercentage = input<boolean>(true);

  /**
   * Enable animated gradient effect
   */
  animated = input<boolean>(true);

  /**
   * Height of the progress bar
   */
  height = input<string>('24px');

  /**
   * Visual state/status
   */
  status = input<'processing' | 'complete' | 'error'>('processing');

  /**
   * Clamped value between 0-100
   */
  clampedValue = computed(() => {
    const val = this.value();
    return Math.min(Math.max(val, 0), 100);
  });

  /**
   * Formatted percentage text
   */
  percentageText = computed(() => `${Math.round(this.clampedValue())}%`);

  /**
   * Dynamic CSS custom properties for styling
   */
  progressStyles = computed(() => ({
    '--progress-value': `${this.clampedValue()}%`,
    '--progress-height': this.height()
  }));

  /**
   * CSS classes based on component state
   */
  containerClasses = computed(() => ({
    'progress-bar-container': true,
    'animated': this.animated(),
    'complete': this.status() === 'complete',
    'error': this.status() === 'error'
  }));

  constructor() {  }

}