import './SkeletonLoader.css';

function SkeletonLoader() {
  return (
    <div className="skeleton-loader">
      <div className="skeleton-loader__line skeleton-loader__line--lg"></div>
      <div className="skeleton-loader__line skeleton-loader__line--md"></div>
      <div className="skeleton-loader__line skeleton-loader__line--sm"></div>
    </div>
  );
}

export default SkeletonLoader;
