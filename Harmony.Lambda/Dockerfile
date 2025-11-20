FROM public.ecr.aws/lambda/dotnet:9

COPY --from=publish /app/publish ${LAMBDA_TASK_ROOT}

CMD ["Harmony.Lambda"]
