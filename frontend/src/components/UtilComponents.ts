import { styled, Typography } from "@mui/material";
export const BorderedTypography = styled(Typography)(({ theme }) => ({
    border: '2px solid currentColor',
    borderRadius: theme.shape.borderRadius,
    padding: theme.spacing(1, 2),
    display: 'inline-block'
  }));